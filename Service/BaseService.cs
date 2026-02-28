using Microsoft.EntityFrameworkCore;

using where_we_go.DTO;

namespace where_we_go.Service
{
    public abstract class BaseService
    {
        protected async Task<PaginatedResponseDto<TOut>> ToPaginatedResponseAsync<T, TOut>(
            IQueryable<T> source,
            PaginatedQueryDto query,
            Func<T, TOut> mapper)
        {
            var totalRecords = await source.CountAsync();

            var items = await source
            .Skip((query.Page - 1) * query.PageSizeSave)
            .Take(query.PageSizeSave)
            .ToListAsync();

            var mapped = items.Select(mapper).ToList();
            return new PaginatedResponseDto<TOut>(mapped, query.PageSize, query.Page, totalRecords);
        }
    }
}