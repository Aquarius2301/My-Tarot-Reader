using MediatR;
using MyTarotReader.Application.Common.Models;

namespace MyTarotReader.Application.Features.Tarot;

public record GetAllReadingQuery(int Page, int PageSize, Guid UserId)
    : IRequest<GetAllReadingResult>;

public record GetAllReadingItem(
    Guid Id,
    string CardCode,
    bool IsReversed,
    DateTimeOffset CreatedAt
);

public record GetAllReadingResult(
    List<GetAllReadingItem> Items,
    int Page,
    int PageSize,
    int TotalItems
) : PaginationResponse<GetAllReadingItem>(Items, Page, PageSize, TotalItems);
