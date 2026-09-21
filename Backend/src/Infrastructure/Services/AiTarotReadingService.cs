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

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Builds AI tarot readings by asking Gemini to interpret a spread, then stores the result.
/// </summary>
public class AiTarotReadingService(
    IAppDbContext context,
    IGeminiClient geminiClient,
    IValidator<CreateAiTarotReadingRequest> createAiTarotReadingValidator
) : IAiTarotReadingService
{
    private const int AnswerSummaryMaxLength = 300;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

    private readonly IAppDbContext _context = context;
    private readonly IGeminiClient _geminiClient = geminiClient;
    private readonly IValidator<CreateAiTarotReadingRequest> _createAiTarotReadingValidator =
        createAiTarotReadingValidator;

    public async Task<CreateAiTarotReadingResult> CreateAiTarotReadingAsync(
        CreateAiTarotReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createAiTarotReadingValidator, request);

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

        var entity = new AITarotReading
        {
            UserId = userId,
            Title = answer.Title,
            CardCount = request.CardCount,
            QuestionType = request.Type,
            Answer = rawAnswer,
            AnswerSummary = Truncate(answer.Overview),
            Cards = JsonSerializer.Serialize(
                request.Cards.Select(c => new StoredCard(c.CardCode, c.IsReversed)).ToList(),
                JsonOptions
            ),
        };

        _context.AITarotReadings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

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
                .Select(
                    r =>
                        new
                        {
                            r.Id,
                            r.CardCount,
                            r.QuestionType,
                            r.Title,
                            r.Answer,
                            r.Cards,
                            r.CreatedAt,
                        }
                )
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(AiTarotErrorCode.ReadingNotFound);

        return new GetAiTarotReadingResult(
            reading.Id,
            reading.CardCount,
            reading.QuestionType,
            reading.Title,
            reading.Answer,
            DeserializeCards(reading.Cards),
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
            .Select(
                r =>
                    new
                    {
                        r.Id,
                        r.CardCount,
                        r.QuestionType,
                        r.Title,
                        r.AnswerSummary,
                        r.Cards,
                        r.CreatedAt,
                    }
            )
            .ToListAsync(cancellationToken);

        var items = readings
            .Select(
                r =>
                    new GetAllAiTarotReadingItem(
                        r.Id,
                        r.CardCount,
                        r.QuestionType,
                        r.Title,
                        r.AnswerSummary,
                        DeserializeCards(r.Cards),
                        r.CreatedAt
                    )
            )
            .ToList();

        return new GetAllAiTarotReadingResult(items);
    }

    private static string BuildPrompt(CreateAiTarotReadingRequest request)
    {
        var isEnglish = request.Locale == "en";
        var lines = request
            .Cards.Select((card, index) =>
            {
                var name = TarotConstants.GetCardName(card.CardCode) ?? card.CardCode;
                var orientation = card.IsReversed
                    ? isEnglish
                        ? "Reversed"
                        : "Ngược"
                    : isEnglish
                        ? "Upright"
                        : "Xuôi";
                return $"{index + 1}. {name} ({orientation})";
            })
            .ToList();

        return string.Join(
            "\n",
            isEnglish
                ? "You are an experienced tarot reader."
                : "Bạn là một nhà chiêm tinh tarot giàu kinh nghiệm.",
            isEnglish
                ? $"The user wants a tarot reading about the topic: {request.Type}."
                : $"Người dùng muốn xem tarot về chủ đề: {request.Type}.",
            isEnglish
                ? $"The spread contains {(int)request.CardCount} cards:"
                : $"Trải bài gồm {(int)request.CardCount} lá bài:",
            string.Join("\n", lines),
            "",
            isEnglish
                ? "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:"
                : "Hãy diễn giải trải bài và trả về DUY NHẤT một chuỗi JSON hợp lệ (không kèm bất kỳ văn bản nào khác), theo đúng schema sau:",
            """{ "title": "short title of the reading (5-8 words)", "overview": "brief overview of the overall energy and the issue of the spread", "cards": [ { "cardCode": "the card code", "position": "the position of the card in the spread", "interpretation": "interpretation of the card meaning in the context of the question" } ], "overallAdvice": "overall advice for the user" }""",
            "",
            isEnglish ? "Respond in English." : "Phản hồi bằng tiếng Việt."
        );
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