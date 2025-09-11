using Vendas.API.Domain.Services.Communication;

namespace Vendas.API.DTOs.Response;

public record PaginationInfo
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int CurrentCount { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }

    public PaginationInfo(int page, int pageSize, int totalCount, int currentCount)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = CalculateTotalPages(totalCount, pageSize);
        CurrentCount = currentCount;
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }

    public object ToMetaObject() => new
    {
        page = Page,
        pageSize = PageSize,
        totalCount = TotalCount,
        totalPages = TotalPages,
        currentCount = CurrentCount,
        hasNextPage = HasNextPage,
        hasPreviousPage = HasPreviousPage
    };

    public static PaginationInfo Create(PagedRequest request, int totalCount, int currentCount)
    {
        return new PaginationInfo(request.Page, request.PageSize, totalCount, currentCount);
    }

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0) return 0;
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
