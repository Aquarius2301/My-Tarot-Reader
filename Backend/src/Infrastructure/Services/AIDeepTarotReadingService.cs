using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Builds deep tarot readings (topic-specific spreads such as the 12 astrological houses
/// or the 12 upcoming calendar months) by asking Gemini to interpret the spread, then stores
/// the result.
/// </summary>
public class AIDeepTarotReadingService(
    IAppDbContext context,
    IGeminiClient geminiClient,
    IWalletService walletService,
    IValidator<CreateTwelveHousesReadingRequest> createTwelveHousesReadingValidator,
    IValidator<CreateTwelveMonthsReadingRequest> createTwelveMonthsReadingValidator
) : IAIDeepTarotReadingService
{
    private const int AnswerSummaryMaxLength = 500;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly IReadOnlyDictionary<string, string> CardCodesByName =
        BuildCardCodesByName();

    private readonly IAppDbContext _context = context;
    private readonly IGeminiClient _geminiClient = geminiClient;
    private readonly IWalletService _walletService = walletService;
    private readonly IValidator<CreateTwelveHousesReadingRequest>
        _createTwelveHousesReadingValidator = createTwelveHousesReadingValidator;
    private readonly IValidator<CreateTwelveMonthsReadingRequest>
        _createTwelveMonthsReadingValidator = createTwelveMonthsReadingValidator;

    public async Task<CreateTwelveHousesReadingResult> CreateTwelveHousesReadingAsync(
        CreateTwelveHousesReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createTwelveHousesReadingValidator, request);

        var readingId = await CreateReadingCoreAsync(
            DeepTarotTopic.TwelveHouses,
            request.Locale,
            request.Cards,
            userId,
            cancellationToken
        );

        return new CreateTwelveHousesReadingResult(readingId);
    }

    public async Task<CreateTwelveMonthsReadingResult> CreateTwelveMonthsReadingAsync(
        CreateTwelveMonthsReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createTwelveMonthsReadingValidator, request);

        var readingId = await CreateReadingCoreAsync(
            DeepTarotTopic.TwelveMonths,
            request.Locale,
            request.Cards,
            userId,
            cancellationToken
        );

        return new CreateTwelveMonthsReadingResult(readingId);
    }

    /// <summary>
    /// The shared create pipeline behind every deep tarot topic: check the red coin balance,
    /// ask Gemini to interpret the topic's spread, persist the answer and charge the topic cost.
    /// </summary>
    private async Task<Guid> CreateReadingCoreAsync(
        DeepTarotTopic topic,
        string locale,
        IReadOnlyList<AiDeepCardRequest> cards,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var cost = DeepTarotConstant.GetCost(topic);

        var balance = await _walletService.GetBalanceAsync(userId, cancellationToken);
        if (balance.RedCoin < cost)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientRedCoin);
        }

        var positions = DeepTarotConstant.GetPositions(topic);

        var prompt = BuildPrompt(topic, locale, cards, positions, DateTimeOffset.UtcNow);
        var rawAnswer = await _geminiClient.GenerateContentAsync(prompt, cancellationToken);

        DeepTarotAnswerJson answer;
        try
        {
            answer =
                JsonSerializer.Deserialize<DeepTarotAnswerJson>(rawAnswer, JsonOptions)
                ?? throw new JsonException("Empty answer.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new InternalServerException(
                AiDeepTarotErrorCode.GenerationFailed,
                innerException: ex
            );
        }

        var normalizedAnswer = NormalizeAnswer(
            answer,
            positions,
            cards.Select(c => c.CardCode).ToList()
        );

        var entity = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = topic,
            Title = answer.Title,
            Answer = normalizedAnswer,
            AnswerSummary = Truncate(answer.Overview),
            Cards = JsonSerializer.Serialize(
                cards.Select(c => new StoredCard(c.CardCode, c.IsReversed)).ToList(),
                JsonOptions
            ),
        };

        _context.AIDeepTarotReadings.Add(entity);

        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);

        await _walletService.DeductRedCoinAsync(
            userId,
            new DeductRedCoinRequest(cost, OrderType.AIDeepTarot),
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<GetAiDeepTarotReadingResult> GetAiDeepTarotReadingByIdAsync(
        Guid userId,
        Guid readingId,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await _context
                .AIDeepTarotReadings.AsNoTracking()
                .Where(r => r.Id == readingId && r.UserId == userId)
                .Select(r => new
                {
                    r.Id,
                    r.Topic,
                    r.Title,
                    r.Answer,
                    r.Cards,
                    r.CreatedAt,
                })
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(AiDeepTarotErrorCode.ReadingNotFound);

        var cards = DeserializeCards(reading.Cards);

        var answerJson = reading.Answer;
        if (
            TryParseAnswerJson(answerJson) is { } parsedAnswer
            && DeepTarotConstant.IsSupported(reading.Topic)
        )
        {
            answerJson = NormalizeAnswer(
                parsedAnswer,
                DeepTarotConstant.GetPositions(reading.Topic),
                [.. cards.Select(c => c.CardCode)]
            );
        }

        return new GetAiDeepTarotReadingResult(
            reading.Id,
            reading.Topic,
            reading.Title,
            answerJson,
            cards,
            reading.CreatedAt
        );
    }

    public async Task<GetAllAiDeepTarotReadingResult> GetAllAiDeepTarotReadingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var readings = await _context
            .AIDeepTarotReadings.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Topic,
                r.Title,
                r.AnswerSummary,
                r.Cards,
                r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var items = readings
            .Select(r => new GetAllAiDeepTarotReadingItem(
                r.Id,
                r.Topic,
                r.Title,
                r.AnswerSummary,
                DeserializeCards(r.Cards),
                r.CreatedAt
            ))
            .ToList();

        return new GetAllAiDeepTarotReadingResult(items);
    }

    public async Task DeleteAiDeepTarotReadingAsync(
        Guid userId,
        Guid readingId,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await _context.AIDeepTarotReadings.FirstOrDefaultAsync(
                r => r.Id == readingId && r.UserId == userId,
                cancellationToken
            ) ?? throw new NotFoundException(AiDeepTarotErrorCode.ReadingNotFound);

        reading.DeletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildPrompt(
        DeepTarotTopic topic,
        string locale,
        IReadOnlyList<AiDeepCardRequest> cards,
        IReadOnlyList<DeepTarotPosition> positions,
        DateTimeOffset createdAt
    )
    {
        var languageName = locale == "vi" ? "Vietnamese" : "English";

        var isTwelveMonths = topic == DeepTarotTopic.TwelveMonths;

        var lines = positions
            .Select(
                (position, index) =>
                {
                    var card = cards[index];
                    var name = TarotConstant.GetCardName(card.CardCode) ?? card.CardCode;
                    var orientation = card.IsReversed ? "Reversed" : "Upright";
                    var monthLabel = isTwelveMonths
                        ? $" ({DeepTarotConstant.GetMonthLabel(position, createdAt)})"
                        : string.Empty;
                    return
                        $"{position.Number}. {position.Name}{monthLabel} [{position.Keywords}] -> {name} ({orientation}), key=\"{position.Key}\"";
                }
            )
            .ToList();

        var positionCount = positions.Count;

        return isTwelveMonths
            ? BuildTwelveMonthsPrompt(positions, lines, languageName, positionCount, createdAt)
            : BuildTwelveHousesPrompt(positions, lines, languageName, positionCount);
    }

    private static string BuildTwelveHousesPrompt(
        IReadOnlyList<DeepTarotPosition> positions,
        IReadOnlyList<string> lines,
        string languageName,
        int positionCount
    ) =>
        string.Join(
            "\n",
            "You are a professional, empathetic, and intuitive Tarot reader who also understands astrological chart houses.",
            $"The user asked for a specialized 12-house tarot reading (the {positionCount} houses of the astrological chart, one card per house).",
            "### RULE 1: HOUSE MEANING IS FIXED",
            "Each position is a specific life area of the astrological chart. Never swap houses, never renumber them, and never merge or skip a house.",
            "- The interpretation of a card depends on its house: read the card in the light of the house themes listed below.",
            "- A reversed card shows a blocked, internalized or shadowed expression of the same house themes (it is NOT a separate meaning).",
            "### RULE 2: TONE & LANGUAGE",
            $"Language: MUST respond in natural, warm, insightful, and accessible {languageName}.",
            "Style: WEAVE the house meanings naturally into a cohesive narrative. DO NOT mention rule names, house numbers as puzzle pieces, or internal prompt mechanics (e.g., do not say 'According to Rule 2...'). Speak directly to the user's heart.",
            "Quality: Ensure EVERY house is given a thorough analysis. Do not rush or skim through houses near the end.",
            "Structure:",
            $"+ Write exactly {positionCount} sections, one per house, in the given order.",
            "+ Each section: `title` is a short human-readable label of the house life area in {languageName} (3-8 words), `interpretation` explains the card inside that house (~90-130 words).",
            "+ Then `overview` summarises the whole chart arc in ~120-180 words, and `overallAdvice` gives a final takeaway (~60-100 words).",
            "",
            "The spread positions and the card drawn on each of them:",
            string.Join("\n", lines),
            "",
            "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:",
            $$"""{ "title": "short title of the reading (5-8 words, in {{languageName}})", "overview": "overall arc of the chart across the 12 houses", "sections": [ { "key": "the position key given in the spread, e.g. \"{{positions[0].Key}}\"", "title": "short label of the house life area", "cardCode": "the card code", "interpretation": "interpretation of the card in the context of that house" } ], "overallAdvice": "overall advice for the user" }"""
        );

    private static string BuildTwelveMonthsPrompt(
        IReadOnlyList<DeepTarotPosition> positions,
        IReadOnlyList<string> lines,
        string languageName,
        int positionCount,
        DateTimeOffset createdAt
    )
    {
        var readingMonth = new DateTimeOffset(
            createdAt.Year,
            createdAt.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero
        ).ToString("MM/yyyy", CultureInfo.InvariantCulture);

        return string.Join(
            "\n",
            "You are a professional, empathetic, and intuitive Tarot reader who specializes in month-by-month forecasting.",
            $"The user asked for a specialized 12-month tarot reading: the {positionCount} CONSECUTIVE calendar months that start in the month AFTER the reading month {readingMonth}, one card per month.",
            "The first month is the month right after the reading month, and the last month is the same calendar month of the following year. Each spread position below carries its exact calendar label in the format MM/yyyy - ALWAYS use that exact label in the section title, never renumber, reorder, merge or skip a month.",
            "### RULE 1: MONTH MEANING IS FIXED",
            "- Interpret each card as the energy and events of its own calendar month only. Never let one month bleed into another.",
            "- Read the card in the light of the month themes listed below, and keep the reading forward-looking and practical.",
            "- A reversed card shows a blocked, internalized or shadowed expression of the same month themes (it is NOT a separate meaning).",
            "### RULE 2: TONE & LANGUAGE",
            $"Language: MUST respond in natural, warm, insightful, and accessible {languageName}.",
            "Style: WEAVE the months naturally into one continuous year-long story. DO NOT mention rule names, prompt mechanics, or treat the months as unrelated puzzle pieces (e.g., do not say 'According to Rule 2...'). Speak directly to the user's heart.",
            "Quality: Ensure EVERY month is given a thorough analysis. Do not rush or skim through the final months of the year.",
            "Structure:",
            $"+ Write exactly {positionCount} sections, one per month, in the given order.",
            $"+ Each section: `title` MUST start with the exact calendar label of that month as given in the spread, in the format MM/yyyy, optionally followed by a short theme in {languageName} (e.g. \"10/2026 - Khởi đầu mới\"). `interpretation` explains the card within that month (~90-130 words).",
            "+ Then `overview` summarises the whole year-long arc in ~120-180 words, and `overallAdvice` gives a final takeaway (~60-100 words).",
            "",
            "The spread positions (with their calendar month labels) and the card drawn on each of them:",
            string.Join("\n", lines),
            "",
            "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:",
            $$"""{ "title": "short title of the reading (5-8 words, in {{languageName}})", "overview": "overall arc of the year across the 12 months", "sections": [ { "key": "the position key given in the spread, e.g. \"{{positions[0].Key}}\"", "title": "the calendar month label (MM/yyyy) plus a short theme", "cardCode": "the card code", "interpretation": "interpretation of the card within that month" } ], "overallAdvice": "overall advice for the user" }"""
        );
    }

    private static List<AiDeepReadingCard> DeserializeCards(string cardsJson)
    {
        try
        {
            var cards = JsonSerializer.Deserialize<List<AiDeepReadingCard>>(cardsJson, JsonOptions);
            return cards ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeAnswer(
        DeepTarotAnswerJson answer,
        IReadOnlyList<DeepTarotPosition> positions,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        NormalizeAnswerSections(answer, positions, drawnCardCodes);
        return JsonSerializer.Serialize(answer, JsonOptions);
    }

    private static DeepTarotAnswerJson? TryParseAnswerJson(string rawAnswer)
    {
        try
        {
            return JsonSerializer.Deserialize<DeepTarotAnswerJson>(rawAnswer, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void NormalizeAnswerSections(
        DeepTarotAnswerJson answer,
        IReadOnlyList<DeepTarotPosition> positions,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        if (answer.Sections is null)
        {
            return;
        }

        var knownKeys = positions.ToDictionary(p => p.Key, StringComparer.Ordinal);
        var usedCards = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < answer.Sections.Count; i++)
        {
            var section = answer.Sections[i];
            var key =
                section.Key is not null && knownKeys.ContainsKey(section.Key)
                    ? section.Key
                    : i < positions.Count ? positions[i].Key : section.Key ?? string.Empty;

            var resolvedCard = ResolveCardCode(
                section.CardCode,
                drawnCardCodes,
                usedCards,
                i
            );
            usedCards.Add(resolvedCard);

            answer.Sections[i] = section with
            {
                Key = key,
                CardCode = resolvedCard,
            };
        }
    }

    private static string ResolveCardCode(
        string? rawCardCode,
        IReadOnlyList<string> drawnCardCodes,
        HashSet<string> used,
        int index
    )
    {
        if (TarotConstant.IsValidCardCode(rawCardCode))
        {
            return rawCardCode!;
        }

        var normalizedName = NormalizeCardName(rawCardCode);

        foreach (var drawnCode in drawnCardCodes)
        {
            if (
                !used.Contains(drawnCode)
                && TarotConstant.GetCardName(drawnCode) is { } drawnName
                && NormalizeCardName(drawnName) == normalizedName
            )
            {
                return drawnCode;
            }
        }

        if (
            normalizedName.Length > 0
            && CardCodesByName.TryGetValue(normalizedName, out var deckCode)
        )
        {
            return deckCode;
        }

        return index < drawnCardCodes.Count ? drawnCardCodes[index] : rawCardCode ?? string.Empty;
    }

    private static string NormalizeCardName(string? value) =>
        new string((value ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static IReadOnlyDictionary<string, string> BuildCardCodesByName()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (code, card) in TarotConstant.AllCards)
        {
            map.TryAdd(NormalizeCardName(card.Name), code);
        }

        return map;
    }

    private static string Truncate(string value)
    {
        if (value.Length <= AnswerSummaryMaxLength)
        {
            return value;
        }

        return value[..AnswerSummaryMaxLength];
    }

    private record StoredCard(string CardCode, bool IsReversed);

    private record DeepTarotSectionJson(
        string Key,
        string Title,
        string CardCode,
        string Interpretation
    );

    private record DeepTarotAnswerJson(
        string Title,
        string Overview,
        List<DeepTarotSectionJson> Sections,
        string OverallAdvice
    );
}
