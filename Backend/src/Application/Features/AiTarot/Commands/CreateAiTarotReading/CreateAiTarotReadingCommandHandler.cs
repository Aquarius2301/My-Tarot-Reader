using System;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiTarot.Commands.CreateAiTarotReading;

public class CreateAiTarotReadingCommandHandler(
    IAppDbContext context,
    IWalletService walletService,
    IGeminiClientService geminiClientService,
    IOptions<WalletSetting> walletSettings,
    IOptions<AiTarotSetting> aiTarotSettings
) : IRequestHandler<CreateAiTarotReadingCommand, Guid>
{
    private readonly WalletSetting _walletSetting = walletSettings.Value;
    private readonly AiTarotSetting _aiTarotSetting = aiTarotSettings.Value;

    public async Task<Guid> Handle(
        CreateAiTarotReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var cost = GetCost(request.CardCount);

        var balance = await walletService.GetCoinsAsync(request.UserId, cancellationToken);
        if (balance.WhiteCoins < cost)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientCoins);
        }

        var prompt = BuildPrompt(request);
        var rawAnswer = await geminiClientService.GenerateContentAsync(prompt, cancellationToken);

        AiTarotAnswerJson answer;
        try
        {
            answer =
                JsonSerializer.Deserialize<AiTarotAnswerJson>(rawAnswer, AiTarotHelper.JsonOptions)
                ?? throw new JsonException("Empty answer.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new InternalServerException(
                AiTarotErrorCode.GenerationFailed,
                innerException: ex
            );
        }

        var normalizedAnswer = AiTarotHelper.NormalizeAnswer(
            answer,
            [.. request.Cards.Select(c => c.CardCode)]
        );

        var entity = new AITarotReading
        {
            UserId = request.UserId,
            Title = answer.Title,
            CardCount = request.CardCount,
            QuestionType = request.Type,
            Answer = normalizedAnswer,
            AnswerSummary = AiTarotHelper.Truncate(answer.Overview),
            Cards = JsonSerializer.Serialize(
                request.Cards.Select(c => new AiReadingCard(c.CardCode, c.IsReversed)).ToList(),
                AiTarotHelper.JsonOptions
            ),
        };

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        context.AITarotReadings.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        await walletService.DeductCoinsAsync(
            request.UserId,
            0,
            cost,
            OrderType.AITarot,
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return entity.Id;
    }

    private static string BuildPrompt(CreateAiTarotReadingCommand request)
    {
        var languageName = request.Locale == "vi" ? "Vietnamese" : "English";
        var lines = request
            .Cards.Select(
                (card, index) =>
                {
                    var name = TarotConstant.GetCardName(card.CardCode) ?? card.CardCode;
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
}
