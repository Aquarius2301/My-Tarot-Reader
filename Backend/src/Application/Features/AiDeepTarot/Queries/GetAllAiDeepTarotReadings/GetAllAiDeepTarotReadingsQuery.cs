using MediatR;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// Retrieves a page of deep tarot readings for the user, without the full answer.
/// </summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="UserId">The authenticated user's ID.</param>
public record GetAllAiDeepTarotReadingsQuery(int Page, int PageSize, Guid UserId)
    : IRequest<GetAllAiDeepTarotReadingsResult>;

/// <summary>
/// Result of retrieving a page of deep tarot readings for a user.
/// </summary>
/// <param name="Items">The readings of the requested page.</param>
/// <param name="Page">The normalized 1-based page number.</param>
/// <param name="PageSize">The normalized number of items per page.</param>
/// <param name="Total">The total number of readings across all pages.</param>
public record GetAllAiDeepTarotReadingsResult(
    List<GetAllAiDeepTarotReadingItem> Items,
    int Page,
    int PageSize,
    int Total
) : PaginationResponse<GetAllAiDeepTarotReadingItem>(Items, Page, PageSize, Total);

/// <summary>
/// A single deep tarot reading entry in the user's reading history.
/// </summary>
/// <param name="Id">The reading ID.</param>
/// <param name="Topic">The specialized topic of the reading.</param>
/// <param name="Title">The AI-generated title of the reading.</param>
/// <param name="AnswerSummary">A short excerpt of the answer (not the full answer).</param>
/// <param name="Cards">The drawn cards.</param>
/// <param name="CreatedAt">The creation time of the reading.</param>
/// <param name="Question">The question the user asked, only set by spreads that accept free text.</param>
public record GetAllAiDeepTarotReadingItem(
    Guid Id,
    DeepTarotTopic Topic,
    string Title,
    string AnswerSummary,
    List<AiDeepReadingCard> Cards,
    DateTimeOffset CreatedAt,
    string? Question = null
);
