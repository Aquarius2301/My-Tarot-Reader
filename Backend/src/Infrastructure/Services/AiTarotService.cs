using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Constants;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Performs AI-powered tarot readings backed by the Google Gemini REST API.
/// </summary>
public class AiTarotService : IAiTarotService
{
    private const string GeminiEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";
    private const int AnswerMaxLength = 5000;

    private readonly HttpClient _httpClient;
    private readonly IAppDbContext _context;
    private readonly AiTarotSetting _settings;
    private readonly ILogger<AiTarotService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiTarotService"/> class.
    /// </summary>
    /// <param name="httpClient">The typed HttpClient used to call Gemini.</param>
    /// <param name="context">The application DbContext for persisting readings.</param>
    /// <param name="settings">The AI tarot settings (API key and model).</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public AiTarotService(
        HttpClient httpClient,
        IAppDbContext context,
        IOptions<AiTarotSetting> settings,
        ILogger<AiTarotService> logger
    )
    {
        _httpClient = httpClient;
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CreateAiTarotReadingResponse> CreateAiTarotReadingAsync(
        CreateAiTarotReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        Validate(request);

        var prompt = BuildPrompt(request);
        var answer = await CallGeminiAsync(prompt, cancellationToken);

        var entity = new AIReadHistory
        {
            UserId = userId,
            CardCount = request.CardCount,
            QuestionType = request.QuestionType,
            Answer = answer.Length > AnswerMaxLength ? answer[..AnswerMaxLength] : answer,
            Cards = JsonSerializer.Serialize(request.Cards),
        };

        _context.AIReadHistories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAiTarotReadingResponse(answer);
    }

    /// <summary>Validates the reading request, throwing on invalid input.</summary>
    private static void Validate(CreateAiTarotReadingRequest request)
    {
        if (request.Cards is null || request.Cards.Count != (int)request.CardCount)
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidCardCount);

        if (request.Cards.Any(c => !TarotConstants.IsValidCardCode(c.Code)))
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidCard);
    }

    /// <summary>Builds the Gemini prompt describing the drawn cards and question.</summary>
    private static string BuildPrompt(CreateAiTarotReadingRequest request)
    {
        var lines = new StringBuilder();
        lines.AppendLine(
            "You are a professional, empathetic, and intuitive Tarot reader. Interpret the tarot reading for the user based on the provided cards, spread type, topic, and question."
                + "### RULE 1: POSITIONAL INTERPRETATION"
                + "The card meaning depends heavily on its spread position:"
                + "- 3 cards: Core energy | Challenges/obstacles | Outcome/advice"
                + "- 5 cards: Core energy | Challenges/obstacles | Your strength | Future | Outcome/advice"
                + "- 7 cards: Core energy | Challenges/obstacles | Your strength | Hidden influences | The way to face it | Future | Outcome/advice"
                + "- 10 cards: Core energy | Challenges/obstacles | What to focus on | Past | Your strength | Near future | Suggested approach | What you need to know | Hopes/fears | Outcome/advice"
                + "### RULE 2: TONE & LANGUAGE"
                + "- Language: MUST respond in natural, warm, insightful, and accessible Vietnamese (Văn phong tinh tế, chữa lành, dễ hiểu)."
                + "- Style: WEAVE the positional meanings naturally into a cohesive narrative. DO NOT mention rule names, positional definitions, or internal prompt mechanics (e.g., do not say 'According to Rule 2...' or 'In position 1 which means core energy...'). Speak directly to the user's heart."
        );
        lines.AppendLine($"Cards drawn: {request.Cards.Count}");
        lines.AppendLine("Cards:");
        foreach (var card in request.Cards)
        {
            var orientation = card.IsReversed ? "reversed" : "upright";
            lines.AppendLine($"  - {TarotConstants.GetCardName(card.Code)} ({orientation})");
        }
        lines.AppendLine($"Question type: {request.QuestionType}");
        lines.AppendLine();
        var languageName = request.Language == Language.En ? "English" : "Vietnamese";
        lines.AppendLine(
            $"Provide a warm, structured interpretation in {languageName}. Address each card and its "
                + "reversed meaning if applicable, then give a final takeaway."
        );

        return lines.ToString();
    }

    /// <summary>Posts the prompt to Gemini and returns the extracted answer text.</summary>
    private async Task<string> CallGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        var url = string.Format(GeminiEndpoint, _settings.Model, _settings.ApiKey);

        var payload = JsonSerializer.Serialize(
            new
            {
                contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } },
            }
        );

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Gemini request failed with status {Status} for model {Model}",
                response.StatusCode,
                _settings.Model
            );
            throw new InternalServerException(ErrorMessageCode.AiTarot.InvalidConfig);
        }

        string json;
        try
        {
            json = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is NotSupportedException or JsonException)
        {
            _logger.LogWarning(ex, "Gemini returned an unreadable response body");
            throw new InternalServerException(ErrorMessageCode.AiTarot.EmptyAnswer);
        }

        var answer = ExtractText(json);
        if (string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogWarning(
                "Gemini returned no readable answer for model {Model}",
                _settings.Model
            );
            throw new InternalServerException(ErrorMessageCode.AiTarot.EmptyAnswer);
        }

        return answer;
    }

    /// <summary>
    /// Extracts the answer text from a Gemini generateContent response
    /// (candidates[0].content.parts[*].text). Returns null when absent.
    /// </summary>
    private static string? ExtractText(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            if (
                !doc.RootElement.TryGetProperty("candidates", out var candidates)
                || candidates.ValueKind != JsonValueKind.Array
                || candidates.GetArrayLength() == 0
                || !candidates[0].TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.ValueKind != JsonValueKind.Array
            )
                return null;

            var texts = parts
                .EnumerateArray()
                .Where(p =>
                    p.ValueKind == JsonValueKind.Object
                    && p.TryGetProperty("text", out var t)
                    && t.ValueKind == JsonValueKind.String
                )
                .Select(p => p.GetProperty("text").GetString())
                .Where(t => !string.IsNullOrWhiteSpace(t));

            return string.Join(Environment.NewLine, texts);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
