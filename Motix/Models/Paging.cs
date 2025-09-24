namespace Motix.Models;

public record PagingParameters(int Page = 1, int PageSize = 10)
{
    public int Skip => Math.Max(0, (Page - 1) * Math.Max(1, PageSize));
    public int Take => Math.Max(1, PageSize);
}
public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize, int TotalCount);