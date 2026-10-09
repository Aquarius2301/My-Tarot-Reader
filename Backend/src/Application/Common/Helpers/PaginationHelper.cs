namespace MyTarotReader.Application.Common.Helpers;

/// <summary>
/// Normalizes incoming paging inputs so list endpoints always query a safe page range.
/// </summary>
public static class PaginationHelper
{
    /// <summary>The default 1-based page number.</summary>
    public const int DefaultPage = 1;

    /// <summary>The default number of items per page.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>The maximum allowed number of items per page.</summary>
    public const int MaxPageSize = 50;

    /// <summary>
    /// Clamps the requested page and page size into a valid range instead of failing the request.
    /// </summary>
    /// <param name="page">The requested 1-based page number.</param>
    /// <param name="pageSize">The requested number of items per page.</param>
    /// <returns>The normalized page and page size.</returns>
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < DefaultPage ? DefaultPage : page;
        var normalizedPageSize =
            pageSize <= 0 ? DefaultPageSize : Math.Clamp(pageSize, 1, MaxPageSize);

        return (normalizedPage, normalizedPageSize);
    }

    /// <summary>
    /// Computes the total number of pages for a given total item count and page size.
    /// </summary>
    /// <param name="total">The total number of items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The total number of pages, 0 when there are no items.</returns>
    public static int GetTotalPages(int total, int pageSize)
    {
        if (total <= 0 || pageSize <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(total / (double)pageSize);
    }
}
