using where_we_go.DTO;

namespace where_we_go.Database
{
    public static class QueryablePaginationExtensions
    {
        public static IQueryable<T> ApplyPagination<T, TQuery>(this IQueryable<T> query, TQuery pageQuery)
            where TQuery : PaginatedQueryDto
        {
            var page = pageQuery.PageSave;
            var pageSize = pageQuery.PageSizeSave;
            var skip = (page - 1) * pageSize;

            return query.Skip(skip).Take(pageSize);
        }
    }
}