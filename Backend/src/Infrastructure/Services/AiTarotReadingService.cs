using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Builds AI tarot readings by asking Gemini to interpret a spread, then stores the result.
/// </summary>
public class AiTarotReadingService(
    IAppDbContext context,
    IGeminiClient geminiClient,
    IWalletService walletService,
    IOptions<AiTarotSetting> aiTarotSetting,
    IValidator<CreateAiTarotReadingRequest> createAiTarotReadingValidator
) : IAiTarotReadingService
{
    private const int AnswerSummaryMaxLength = 300;

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
    private readonly AiTarotSetting _aiTarotSetting = aiTarotSetting.Value;
    private readonly IValidator<CreateAiTarotReadingRequest> _createAiTarotReadingValidator =
        createAiTarotReadingValidator;

    public async Task<CreateAiTarotReadingResult> CreateAiTarotReadingAsync(
        CreateAiTarotReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createAiTarotReadingValidator, request);

        var cost = GetCost(request.CardCount);

        var balance = await _walletService.GetBalanceAsync(userId, cancellationToken);
        if (balance.WhiteCoin < cost)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientCoins);
        }

        var prompt = BuildPrompt(request);
        var rawAnswer = await _geminiClient.GenerateContentAsync(prompt, cancellationToken);

        AiTarotAnswerJson answer;
        try
        {
            answer =
                JsonSerializer.Deserialize<AiTarotAnswerJson>(rawAnswer, JsonOptions)
                ?? throw new JsonException("Empty answer.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new InternalServerException(
                AiTarotErrorCode.GenerationFailed,
                innerException: ex
            );
        }

        var normalizedAnswer = NormalizeAnswer(
            answer,
            request.Cards.Select(c => c.CardCode).ToList()
        );

        var entity = new AITarotReading
        {
            UserId = userId,
            Title = answer.Title,
            CardCount = request.CardCount,
            QuestionType = request.Type,
            Answer = normalizedAnswer,
            AnswerSummary = Truncate(answer.Overview),
            Cards = JsonSerializer.Serialize(
                request.Cards.Select(c => new StoredCard(c.CardCode, c.IsReversed)).ToList(),
                JsonOptions
            ),
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken
        );

        _context.AITarotReadings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _walletService.DeductCoinAsync(
            userId,
            new DeductCoinRequest(cost, OrderType.AITarot),
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return new CreateAiTarotReadingResult(entity.Id);
    }

    public async Task<GetAiTarotReadingResult> GetAiTarotReadingByIdAsync(
        Guid userId,
        Guid readingId,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await _context
                .AITarotReadings.AsNoTracking()
                .Where(r => r.Id == readingId && r.UserId == userId)
                .Select(r => new
                {
                    r.Id,
                    r.CardCount,
                    r.QuestionType,
                    r.Title,
                    r.Answer,
                    r.Cards,
                    r.CreatedAt,
                })
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(AiTarotErrorCode.ReadingNotFound);

        var cards = DeserializeCards(reading.Cards);

        var answerJson = reading.Answer;
        if (TryParseAnswerJson(answerJson) is { } parsedAnswer)
        {
            answerJson = NormalizeAnswer(parsedAnswer, [.. cards.Select(c => c.CardCode)]);
        }

        return new GetAiTarotReadingResult(
            reading.Id,
            reading.CardCount,
            reading.QuestionType,
            reading.Title,
            answerJson,
            cards,
            reading.CreatedAt
        );
    }

    public async Task<GetAllAiTarotReadingResult> GetAllAiTarotReadingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var readings = await _context
            .AITarotReadings.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.CardCount,
                r.QuestionType,
                r.Title,
                r.AnswerSummary,
                r.Cards,
                r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var items = readings
            .Select(r => new GetAllAiTarotReadingItem(
                r.Id,
                r.CardCount,
                r.QuestionType,
                r.Title,
                r.AnswerSummary,
                DeserializeCards(r.Cards),
                r.CreatedAt
            ))
            .ToList();

        return new GetAllAiTarotReadingResult(items);
    }

    private static string BuildPrompt(CreateAiTarotReadingRequest request)
    {
        var languageName = request.Locale == "vi" ? "Vietnamese" : "English";
        var lines = request
            .Cards.Select(
                (card, index) =>
                {
                    var name = TarotConstants.GetCardName(card.CardCode) ?? card.CardCode;
                    var orientation = card.IsReversed ? "Reversed" : "Upright";
                    return $"{index + 1}. {name} ({orientation})";
                }
            )
            .ToList();

        return string.Join(
            "\n",
            "You are a professional, empathetic, and intuitive Tarot reader. Interpret the tarot reading for the user based on the provided cards, spread type, topic, and question.",
            "### RULE 1: POSITIONAL INTERPRETATION",
            "The card meaning depends heavily on its spread position:",
            GetPositionRule(request.CardCount),
            "### RULE 2: TONE & LANGUAGE",
            $"Language: MUST respond in natural, warm, insightful, and accessible in {languageName}.",
            "Style: WEAVE the positional meanings naturally into a cohesive narrative. DO NOT mention rule names, positional definitions, or internal prompt mechanics (e.g., do not say 'According to Rule 2...' or 'In position 1 which means core energy...'). Speak directly to the user's heart.",
            "Quality: Ensure EVERY card is given thorough analysis according to its position. Do not rush or skim through cards near the end.",
            "Structure:",
            "+ Introduction: Hello and introduce the reading, acknowledge the user's question, and set a warm tone. (~50-100 words)",
            "+ Main Analysis: Position name - Card name (upright/reversed): meaning and interpretation. (~100-150 words per card)",
            "+ Conclusion: Summarize the key insights and provide a final takeaway. (~50-100 words)",
            "",
            $"The user wants a tarot reading about the topic: {request.Type}.",
            $"The spread contains {(int)request.CardCount} cards:",
            string.Join("\n", lines),
            "",
            "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:",
            """{ "title": "short title of the reading (5-8 words)", "overview": "brief overview of the overall energy and the issue of the spread", "cards": [ { "cardCode": "the card code", "position": "the position of the card in the spread", "interpretation": "interpretation of the card meaning in the context of the question" } ], "overallAdvice": "overall advice for the user" }"""
        );
    }

    private static string GetPositionRule(CardCount cardCount)
    {
        return cardCount switch
        {
            CardCount.Three =>
                "- 3 cards: Core energy | Challenges/obstacles | Outcome/advice (Overall response ~300-400 words)",
            CardCount.Five =>
                "- 5 cards: Core energy | Challenges/obstacles | Your strength | Future | Outcome/advice (Overall response ~450-600 words)",
            CardCount.Seven =>
                "- 7 cards: Core energy | Challenges/obstacles | Your strength | Hidden influences | The way to face it | Future | Outcome/advice (Overall response ~650-800 words)",
            CardCount.Ten =>
                "- 10 cards: Core energy | Challenges/obstacles | What to focus on | Past | Your strength | Near future | Suggested approach | What you need to know | Hopes/fears | Outcome/advice (Overall response ~800-1000 words)",
            _ => throw new InternalServerException(AiTarotErrorCode.InvalidCardCount),
        };
    }

    private int GetCost(CardCount cardCount)
    {
        if (!_aiTarotSetting.Costs.TryGetValue((int)cardCount, out var cost))
        {
            throw new InternalServerException(AiTarotErrorCode.InvalidCardCount);
        }

        return cost;
    }

    private static List<AiReadingCard> DeserializeCards(string cardsJson)
    {
        try
        {
            var cards = JsonSerializer.Deserialize<List<AiReadingCard>>(cardsJson, JsonOptions);
            return cards ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeAnswer(
        AiTarotAnswerJson answer,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        NormalizeAnswerCardCodes(answer, drawnCardCodes);
        return JsonSerializer.Serialize(answer, JsonOptions);
    }

    private static AiTarotAnswerJson? TryParseAnswerJson(string rawAnswer)
    {
        try
        {
            return JsonSerializer.Deserialize<AiTarotAnswerJson>(rawAnswer, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void NormalizeAnswerCardCodes(
        AiTarotAnswerJson answer,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        if (answer.Cards is null || drawnCardCodes.Count == 0)
        {
            return;
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < answer.Cards.Count; i++)
        {
            var card = answer.Cards[i];
            var resolved = ResolveCardCode(card.CardCode, drawnCardCodes, used, i);
            used.Add(resolved);
            answer.Cards[i] = card with { CardCode = resolved };
        }
    }

    private static string ResolveCardCode(
        string rawCardCode,
        IReadOnlyList<string> drawnCardCodes,
        HashSet<string> used,
        int index
    )
    {
        if (TarotConstants.IsValidCardCode(rawCardCode))
        {
            return rawCardCode;
        }

        var normalizedName = NormalizeCardName(rawCardCode);

        foreach (var drawnCode in drawnCardCodes)
        {
            if (
                !used.Contains(drawnCode)
                && TarotConstants.GetCardName(drawnCode) is { } drawnName
                && NormalizeCardName(drawnName) == normalizedName
            )
            {
                return drawnCode;
            }
        }

        if (CardCodesByName.TryGetValue(normalizedName, out var deckCode))
        {
            return deckCode;
        }

        return index < drawnCardCodes.Count ? drawnCardCodes[index] : rawCardCode;
    }

    private static string NormalizeCardName(string? value) =>
        new string((value ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static IReadOnlyDictionary<string, string> BuildCardCodesByName()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (code, card) in TarotConstants.AllCards)
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

    private record AiTarotCardJson(string CardCode, string Position, string Interpretation);

    private record AiTarotAnswerJson(
        string Title,
        string Overview,
        List<AiTarotCardJson> Cards,
        string OverallAdvice
    );
}
