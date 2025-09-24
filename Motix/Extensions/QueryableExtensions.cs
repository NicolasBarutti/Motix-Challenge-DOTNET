using Microsoft.EntityFrameworkCore;
using Motix.Models;

namespace Motix.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, PagingParameters paging, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(paging.Skip).Take(paging.Take).ToListAsync(ct);
        return new PagedResult<T>(items, paging.Page, paging.PageSize, total);
    }
}