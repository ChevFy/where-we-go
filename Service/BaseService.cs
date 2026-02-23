using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;

namespace where_we_go.Service
{
    public abstract class BaseService
    {
        protected async Task<PaginatedResponseDto<TOut>> ToPaginatedResponseAsync<TEntity, TOut, TQuery>(
            IQueryable<TEntity> source,
            TQuery query,
            Func<TEntity, TOut> mapper,
            CancellationToken cancellationToken = default)
            where TQuery : PaginatedQueryDto
        {
            var page = query.PageSave;
            var pageSize = query.PageSizeSave;

            var totalCount = await source.CountAsync(cancellationToken);
            var entities = await source
                .ApplyPagination(query)
                .ToListAsync(cancellationToken);

            var data = entities.Select(mapper).ToList();

            return new PaginatedResponseDto<TOut>(data, pageSize, page, totalCount);
        }
    }
}