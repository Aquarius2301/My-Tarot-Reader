using MediatR;
using MyTarotReader.Application.Common.Models;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAllAiTarotReadingQuery(int Page, int PageSize, Guid UserId)
    : IRequest<GetAllAiTarotReadingResult>;

public record GetAllAiTarotReadingResult(
    List<GetAllAiTarotReadingItem> Items,
    int Page,
    int PageSize,
    int Total
) : PaginationResponse<GetAllAiTarotReadingItem>(Items, Page, PageSize, Total);

public record GetAllAiTarotReadingItem(
    Guid Id,
    int CardCount,
    string Type,
    string Title,
    string AnswerSummary,
    List<AiReadingCard> Cards,
    DateTimeOffset CreatedAt
);
