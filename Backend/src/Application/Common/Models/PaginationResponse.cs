namespace MyTarotReader.Application.Common.Models;

public record PaginationResponse<T>(List<T> Items, int Page, int PageSize, int TotalItems)
{
    int TotalPages { get; init; } =
        PageSize < 0 || TotalItems < 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
