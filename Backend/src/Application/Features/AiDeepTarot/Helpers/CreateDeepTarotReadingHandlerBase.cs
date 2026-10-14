using System.Text.Json;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// The shared create pipeline behind every deep tarot command: check the red coin balance,
/// ask Gemini to interpret the spread, persist the answer and charge the topic cost inside a
/// single transaction.
/// </summary>
/// <remarks>
/// The red coin balance is checked before Gemini is called, so a request that cannot be paid
/// for never burns AI tokens.
/// </remarks>
public abstract class CreateDeepTarotReadingHandlerBase(
    IAppDbContext context,
    IGeminiClientService geminiClientService,
    IWalletService walletService
)
{
    /// <summary>
    /// Ensures the user owns enough red coins for the reading before any AI token is spent.
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when the red coin balance is too low.</exception>
    protected async Task EnsureBalanceAsync(
        Guid userId,
        int cost,
        CancellationToken cancellationToken
    )
    {
        var balance = await walletService.GetCoinsAsync(userId, cancellationToken);
        if (balance.RedCoins < cost)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientRedCoins);
        }
    }

    /// <summary>
    /// Asks Gemini, normalizes the answer, persists it and charges the cost.
    /// </summary>
    protected async Task<Guid> CreateReadingAsync(
        DeepTarotTopic topic,
        string prompt,
        IReadOnlyList<DeepTarotPosition> positions,
        int cost,
        IReadOnlyList<AiDeepCardRequest> cards,
        Guid userId,
        CancellationToken cancellationToken,
        string? question = null,
        List<string>? options = null,
        CrossroadsTimeFrame? timeFrame = null
    )
    {
        await EnsureBalanceAsync(userId, cost, cancellationToken);

        var rawAnswer = await geminiClientService.GenerateContentAsync(prompt, cancellationToken);

        DeepTarotAnswerJson answer;
        try
        {
            answer =
                JsonSerializer.Deserialize<DeepTarotAnswerJson>(rawAnswer, AiDeepTarotHelper.JsonOptions)
                ?? throw new JsonException("Empty answer.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new InternalServerException(
                AiDeepTarotErrorCode.GenerationFailed,
                innerException: ex
            );
        }

        AiDeepTarotHelper.ThrowIfRefused(answer);

        var normalizedAnswer = AiDeepTarotHelper.NormalizeAnswer(
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
            AnswerSummary = AiDeepTarotHelper.Truncate(answer.Overview),
            Cards = AiDeepTarotHelper.SerializeCards(cards),
            Question = question,
            Options =
                options is null
                    ? null
                    : JsonSerializer.Serialize(options, AiDeepTarotHelper.JsonOptions),
            TimeFrame = timeFrame,
        };

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        context.AIDeepTarotReadings.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        await walletService.DeductCoinsAsync(
            userId,
            cost,
            0,
            OrderType.AIDeepTarot,
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return entity.Id;
    }
}
