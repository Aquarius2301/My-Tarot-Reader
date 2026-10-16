using System;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiTarot;

public class CreateAiTarotReadingCommandHandler(
    IAppDbContext context,
    IWalletService walletService,
    IGeminiClientService geminiClientService
) : IRequestHandler<CreateAiTarotReadingCommand, CreateAiTarotReadingResult>
{
    public async Task<CreateAiTarotReadingResult> Handle(
        CreateAiTarotReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var setting =
            await context
                .AITarotReadingCounts.AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.CardCount == request.CardCount && s.IsEnabled,
                    cancellationToken
                )
            ?? throw new BadRequestException(AiTarotErrorCode.InvalidCardCount);

        var readingType =
            await context
                .AITarotReadingTypes.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Code == request.Type && s.IsEnabled, cancellationToken)
            ?? throw new BadRequestException(AiTarotErrorCode.InvalidQuestionType);

        var cost = setting.WhiteCoins;

        var balance = await walletService.GetCoinsAsync(request.UserId, cancellationToken);
        if (balance.WhiteCoins < cost)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientCoins);
        }

        var prompt = BuildPrompt(request, setting, readingType);
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

        return new CreateAiTarotReadingResult(entity.Id);
    }

    private static string BuildPrompt(
        CreateAiTarotReadingCommand request,
        AITarotReadingCount setting,
        AiTarotReadingType readingType
    )
    {
        var isVietnamese = request.Locale == "vi";
        var languageName = isVietnamese ? "Vietnamese" : "English";
        var positions = isVietnamese ? setting.PositionsVi : setting.PositionsEn;
        var topicName = isVietnamese ? readingType.NameVi : readingType.NameEn;

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
            GetPositionRule(setting, positions),
            "### RULE 2: TONE & LANGUAGE",
            $"Language: MUST respond in natural, warm, insightful, and accessible in {languageName}.",
            "Style: WEAVE the positional meanings naturally into a cohesive narrative. DO NOT mention rule names, positional definitions, or internal prompt mechanics (e.g., do not say 'According to Rule 2...' or 'In position 1 which means core energy...'). Speak directly to the user's heart.",
            "Quality: Ensure EVERY card is given thorough analysis according to its position. Do not rush or skim through cards near the end.",
            "Structure:",
            "+ Introduction: Hello and introduce the reading, acknowledge the user's question, and set a warm tone. (~50-100 words)",
            "+ Main Analysis: Position name - Card name (upright/reversed): meaning and interpretation. (~100-150 words per card)",
            "+ Conclusion: Summarize the key insights and provide a final takeaway. (~50-100 words)",
            "",
            $"The user wants a tarot reading about the topic: {topicName}.",
            $"The spread contains {request.CardCount} cards:",
            string.Join("\n", lines),
            "",
            "Interpret the spread and return ONLY one valid JSON string (no other text), according to this exact schema:",
            """{ "title": "short title of the reading (5-8 words)", "overview": "brief overview of the overall energy and the issue of the spread", "cards": [ { "cardCode": "the card code", "position": "the position of the card in the spread", "interpretation": "interpretation of the card meaning in the context of the question" } ], "overallAdvice": "overall advice for the user" }"""
        );
    }

    private static string GetPositionRule(AITarotReadingCount setting, List<string> positions)
    {
        return $"- {setting.CardCount} cards: {string.Join(" | ", positions)} (Overall response ~{setting.MinWords}-{setting.MaxWords} words)";
    }
}
