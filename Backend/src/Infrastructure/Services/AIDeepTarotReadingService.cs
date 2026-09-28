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
/// Builds deep tarot readings (topic-specific spreads such as the 12 astrological houses)
/// by asking Gemini to interpret the spread, then stores the result.
/// </summary>
public class AIDeepTarotReadingService(
    IAppDbContext context,
    IGeminiClient geminiClient,
    IValidator<CreateAiDeepTarotReadingRequest> createAiDeepTarotReadingValidator
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
    private readonly IValidator<CreateAiDeepTarotReadingRequest> _createAiDeepTarotReadingValidator =
        createAiDeepTarotReadingValidator;

    public async Task<CreateAiDeepTarotReadingResult> CreateAiDeepTarotReadingAsync(
        CreateAiDeepTarotReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createAiDeepTarotReadingValidator, request);

        var positions = DeepTarotConstant.GetPositions(request.Topic);

        var prompt = BuildPrompt(request, positions);
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
            request.Cards.Select(c => c.CardCode).ToList()
        );

        var entity = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = request.Topic,
            Title = answer.Title,
            Answer = normalizedAnswer,
            AnswerSummary = Truncate(answer.Overview),
            Cards = JsonSerializer.Serialize(
                request
                    .Cards.Select(c => new StoredCard(c.CardCode, c.IsReversed))
                    .ToList(),
                JsonOptions
            ),
        };

        _context.AIDeepTarotReadings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAiDeepTarotReadingResult(entity.Id);
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
        CreateAiDeepTarotReadingRequest request,
        IReadOnlyList<DeepTarotPosition> positions
    )
    {
        var languageName = request.Locale == "vi" ? "Vietnamese" : "English";

        var lines = positions
            .Select(
                (position, index) =>
                {
                    var card = request.Cards[index];
                    var name = TarotConstant.GetCardName(card.CardCode) ?? card.CardCode;
                    var orientation = card.IsReversed ? "Reversed" : "Upright";
                    return
                        $"{position.Number}. {position.Name} [{position.Keywords}] -> {name} ({orientation}), key=\"{position.Key}\"";
                }
            )
            .ToList();

        return string.Join(
            "\n",
            "You are a professional, empathetic, and intuitive Tarot reader who also understands astrological chart houses.",
            $"The user asked for a specialized 12-house tarot reading (the {positions.Count} houses of the astrological chart, one card per house).",
            "### RULE 1: HOUSE MEANING IS FIXED",
            "Each position is a specific life area of the astrological chart. Never swap houses, never renumber them, and never merge or skip a house.",
            "- The interpretation of a card depends on its house: read the card in the light of the house themes listed below.",
            "- A reversed card shows a blocked, internalized or shadowed expression of the same house themes (it is NOT a separate meaning).",
            "### RULE 2: TONE & LANGUAGE",
            $"Language: MUST respond in natural, warm, insightful, and accessible {languageName}.",
            "Style: WEAVE the house meanings naturally into a cohesive narrative. DO NOT mention rule names, house numbers as puzzle pieces, or internal prompt mechanics (e.g., do not say 'According to Rule 2...'). Speak directly to the user's heart.",
            "Quality: Ensure EVERY house is given a thorough analysis. Do not rush or skim through houses near the end.",
            "Structure:",
            $"+ Write exactly {positions.Count} sections, one per house, in the given order.",
            "+ Each section: `title` is a short human-readable label of the house life area in {languageName} (3-8 words), `interpretation` explains the card inside that house (~90-130 words).",
            "+ Then `overview` summarises the whole chart arc in ~120-180 words, and `overallAdvice` gives a final takeaway (~60-100 words).",
            "",
            "The spread positions and the card drawn on each of them:",
            string.Join("\n", lines),
            "",
            "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:",
            $$"""{ "title": "short title of the reading (5-8 words, in {{languageName}})", "overview": "overall arc of the chart across the 12 houses", "sections": [ { "key": "the position key given in the spread, e.g. \"{{positions[0].Key}}\"", "title": "short label of the house life area", "cardCode": "the card code", "interpretation": "interpretation of the card in the context of that house" } ], "overallAdvice": "overall advice for the user" }"""
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
