using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
/// Handles custom AI tarot chat sessions backed by the Google Gemini REST API.
/// </summary>
public class AiChatService : IAiChatService
{
    private const string GeminiEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";
    private const int AnswerMaxLength = 5000;
    private const int MaxConversationMessages = 50;
    private const int MaxCustomCards = 15;
    private const int MinCustomCards = 1;

    private readonly HttpClient _httpClient;
    private readonly IAppDbContext _context;
    private readonly AiTarotSetting _settings;
    private readonly ILogger<AiChatService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiChatService"/> class.
    /// </summary>
    /// <param name="httpClient">The typed HttpClient used to call Gemini.</param>
    /// <param name="context">The application DbContext for persisting sessions.</param>
    /// <param name="settings">The AI tarot settings (API key and model).</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public AiChatService(
        HttpClient httpClient,
        IAppDbContext context,
        IOptions<AiTarotSetting> settings,
        ILogger<AiChatService> logger
    )
    {
        _httpClient = httpClient;
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Flow:
    /// 1. Validate the request.
    /// 2. Call Gemini with a single user message and the chat system instruction.
    /// 3. Persist the new AIReadHistory entity with Status = Chat.
    /// 4. Persist the initial ChatMessage records.
    /// 5. Return the history id and the AI's initial response.
    /// </remarks>
    public async Task<CreateChatSessionResponse> CreateChatSessionAsync(
        CreateChatSessionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new BadRequestException(ErrorMessageCode.AiTarot.EmptyConversation);

        var systemInstruction = BuildChatSystemInstruction(request.Language);
        var answer = await CallGeminiAsync(
            systemInstruction,
            new[] { ("user", request.Question) },
            cancellationToken
        );

        var history = new AIReadHistory
        {
            UserId = userId,
            Question = request.Question,
            Status = ChatSessionStatus.Chat,
        };

        _context.AIReadHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        _context.ChatMessages.AddRange(
            new ChatMessage
            {
                HistoryId = history.Id,
                Role = "user",
                Text = request.Question,
                Sequence = 0,
            },
            new ChatMessage
            {
                HistoryId = history.Id,
                Role = "model",
                Text = answer,
                Sequence = 1,
            }
        );
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateChatSessionResponse(history.Id, answer);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Flow:
    /// 1. Load and validate the reading history (ownership + phase).
    /// 2. Load conversation messages from DB.
    /// 3. Call Gemini with the full conversation history + new message.
    /// 4. Persist the new ChatMessage records.
    /// 5. Persist and return the AI's response.
    /// </remarks>
    public async Task<SendChatMessageResponse> SendChatMessageAsync(
        SendChatMessageRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new BadRequestException(ErrorMessageCode.AiTarot.EmptyConversation);

        var history = await LoadHistoryAsync(request.HistoryId, userId, cancellationToken);

        if (history.Status == ChatSessionStatus.Reading)
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidSessionPhase);

        var messages = await _context
            .ChatMessages.Where(m => m.HistoryId == history.Id)
            .OrderBy(m => m.Sequence)
            .ToListAsync(cancellationToken);

        if (messages.Count > MaxConversationMessages)
            throw new BadRequestException(ErrorMessageCode.AiTarot.ConversationTooLong);

        var geminiMessages = messages
            .Select(m => (m.Role, m.Text))
            .Concat(new[] { ("user", request.Message) })
            .ToList();

        var systemInstruction = BuildChatSystemInstruction(request.Language);
        var answer = await CallGeminiAsync(systemInstruction, geminiMessages, cancellationToken);

        var nextSeq = messages.Count;
        _context.ChatMessages.AddRange(
            new ChatMessage
            {
                HistoryId = history.Id,
                Role = "user",
                Text = request.Message,
                Sequence = nextSeq,
            },
            new ChatMessage
            {
                HistoryId = history.Id,
                Role = "model",
                Text = answer,
                Sequence = nextSeq + 1,
            }
        );
        await _context.SaveChangesAsync(cancellationToken);

        var spread = TryParseSpreadRecommendation(answer);

        return new SendChatMessageResponse(answer, spread);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Flow:
    /// 1. Validate card count (1-15) and card codes.
    /// 2. Load and validate the reading history (ownership + phase).
    /// 3. Load conversation messages for prompt building.
    /// 4. Build the reading prompt with chat context + cards.
    /// 5. Call Gemini for interpretation.
    /// 6. Update the AIReadHistory with reading results.
    /// 7. Persist and return the answer.
    /// </remarks>
    public async Task<CreateCustomReadingResponse> CreateCustomReadingAsync(
        CreateCustomReadingRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (
            request.Cards is null
            || request.Cards.Count < MinCustomCards
            || request.Cards.Count > MaxCustomCards
        )
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidCustomCardCount);

        if (request.Cards.Any(c => !TarotConstants.IsValidCardCode(c.Code)))
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidCard);

        var history = await LoadHistoryAsync(request.HistoryId, userId, cancellationToken);

        if (history.Status == ChatSessionStatus.Reading)
            throw new BadRequestException(ErrorMessageCode.AiTarot.InvalidSessionPhase);

        var conversationLines = await _context
            .ChatMessages.Where(m => m.HistoryId == history.Id)
            .OrderBy(m => m.Sequence)
            .Select(m => $"{m.Role}: {m.Text}")
            .ToListAsync(cancellationToken);

        var prompt = BuildCustomReadingPrompt(
            history,
            conversationLines,
            request.Cards,
            request.Language
        );
        var answer = await CallGeminiAsync(
            BuildChatSystemInstruction(request.Language),
            new[] { ("user", prompt) },
            cancellationToken
        );

        var truncatedAnswer = answer.Length > AnswerMaxLength ? answer[..AnswerMaxLength] : answer;

        history.Status = ChatSessionStatus.Reading;
        history.CardCount = null; // Not applicable for custom readings
        history.QuestionType = QuestionType.Custom;
        history.Answer = truncatedAnswer;
        history.Cards = JsonSerializer.Serialize(request.Cards);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCustomReadingResponse(answer);
    }

    /// <summary>Loads a reading history record and validates ownership.</summary>
    private async Task<AIReadHistory> LoadHistoryAsync(
        Guid historyId,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        return await _context.AIReadHistories.FirstOrDefaultAsync(
                h => h.Id == historyId && h.UserId == userId && h.DeletedAt == null,
                cancellationToken
            ) ?? throw new NotFoundException(ErrorMessageCode.AiTarot.SessionNotFound);
    }

    /// <summary>Builds the system instruction for the chatbot role.</summary>
    private static string BuildChatSystemInstruction(Language language)
    {
        var languageName = language == Language.En ? "English" : "Vietnamese";

        return "You are a professional, empathetic, and intuitive Tarot reader. You guide the user through a deeply personal consultation."
            + "\n\n### YOUR ROLE"
            + "\n- Listen carefully to the user's question and situation"
            + "\n- Ask follow-up questions one at a time to understand their situation, emotions, and what they seek clarity on"
            + "\n- Do NOT rush to recommend a spread — take time to understand deeply (2-6 exchanges)"
            + "\n- Be warm, insightful, and accessible in your tone"
            + "\n\n### SPREAD RECOMMENDATION"
            + "\nAfter you have sufficient understanding (usually 2-6 exchanges), propose a specific spread layout:"
            + "\n- State the number of cards (1 to 15)"
            + "\n- Name each position with a specific meaning"
            + "\n- A position name can be used multiple positions if the same meaning applies to multiple cards"
            + "\n- Use this EXACT format when recommending a spread:"
            + "\n\n**Proposed Spread: {Spread Name}** ({N} cards)"
            + "\n\nPosition 1: {position name}"
            + "\nPosition 2: {position name}"
            + "\n..."
            + "\nPosition N: {position name}"
            + "\n\n### RULES"
            + "\n- Language: MUST respond in natural, warm, and accessible "
            + languageName
            + "\n- Style: Speak directly to the user's heart. Be empathetic and intuitive."
            + "\n- Do NOT mention internal rules or prompt mechanics"
            + "\n- When recommending a spread, always use the exact format above so the system can parse it"
            + "\n- Keep follow-up questions focused and meaningful";
    }

    /// <summary>Builds the reading prompt for custom sessions with chat context.</summary>
    private static string BuildCustomReadingPrompt(
        AIReadHistory history,
        List<string> conversationLines,
        List<AiTarotCardRequest> cards,
        Language language
    )
    {
        var lines = new StringBuilder();
        lines.AppendLine(
            "You are now performing a tarot reading based on the following consultation:"
        );
        lines.AppendLine();
        lines.AppendLine($"User's original question: {history.Question}");
        lines.AppendLine();
        lines.AppendLine("Here is a summary of the conversation context from the consultation:");
        lines.AppendLine(string.Join("\n", conversationLines));
        lines.AppendLine();
        lines.AppendLine($"Cards drawn: {cards.Count}");
        lines.AppendLine("Cards:");
        foreach (var card in cards)
        {
            var orientation = card.IsReversed ? "reversed" : "upright";
            lines.AppendLine($"  - {TarotConstants.GetCardName(card.Code)} ({orientation})");
        }
        lines.AppendLine();
        var languageName = language == Language.En ? "English" : "Vietnamese";
        lines.AppendLine(
            $"Provide a warm, structured interpretation in {languageName}. "
                + "Address each card and its reversed meaning if applicable, "
                + "then give a final takeaway. Weave the positional meanings naturally into a cohesive narrative."
        );

        return lines.ToString();
    }

    /// <summary>Sends a request to Gemini with conversation history and returns the extracted answer text.</summary>
    private async Task<string> CallGeminiAsync(
        string systemInstruction,
        IReadOnlyList<(string Role, string Text)> messages,
        CancellationToken cancellationToken
    )
    {
        var url = string.Format(GeminiEndpoint, _settings.Model, _settings.ApiKey);

        var contents = messages
            .Select(m => new { role = m.Role, parts = new[] { new { text = m.Text } } })
            .ToArray();

        var payload = JsonSerializer.Serialize(
            new
            {
                systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                contents,
                generationConfig = new { maxOutputTokens = _settings.MaxOutputTokens },
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
    /// Attempts to parse a spread recommendation from the AI response text.
    /// Looks for the pattern: **Proposed Spread: {Name}** ({N} cards)
    /// followed by Position lines.
    /// </summary>
    private static SpreadRecommendationDto? TryParseSpreadRecommendation(string answer)
    {
        try
        {
            var lines = answer.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                // Match: **Proposed Spread: Some Name** (N cards)
                if (!line.Contains("Proposed Spread", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract spread name
                var nameStart = line.IndexOf(": ", StringComparison.Ordinal);
                var nameEnd = line.IndexOf("**", nameStart + 2, StringComparison.Ordinal);
                if (nameStart < 0 || nameEnd < 0)
                    continue;
                var spreadName = line.Substring(nameStart + 2, nameEnd - nameStart - 2).Trim();

                // Extract card count
                var countStart = line.LastIndexOf('(');
                var countEnd = line.LastIndexOf(')');
                if (countStart < 0 || countEnd < 0)
                    continue;
                var countStr = line.Substring(countStart + 1, countEnd - countStart - 1).Split(' ')[
                    0
                ];
                if (!int.TryParse(countStr, out var cardCount) || cardCount < 1 || cardCount > 15)
                    continue;

                // Parse position lines
                var positions = new List<SpreadPositionDto>();
                for (int j = i + 1; j < lines.Length; j++)
                {
                    var posLine = lines[j].Trim();
                    if (string.IsNullOrEmpty(posLine))
                        continue;
                    if (posLine.StartsWith("Please draw", StringComparison.OrdinalIgnoreCase))
                        break;

                    // Match: Position N: name
                    if (!posLine.StartsWith("Position", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var colonIdx = posLine.IndexOf(':');
                    if (colonIdx < 0)
                        continue;

                    var numStr = posLine.Substring(9, colonIdx - 9).Trim();
                    if (!int.TryParse(numStr, out var posNum))
                        continue;

                    var posName = posLine.Substring(colonIdx + 1).Trim();
                    positions.Add(new SpreadPositionDto(posNum, posName));
                }

                if (positions.Count > 0)
                    return new SpreadRecommendationDto(spreadName, cardCount, positions);
            }
        }
        catch
        {
            // Parsing failed — not a spread recommendation, return null
        }

        return null;
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
