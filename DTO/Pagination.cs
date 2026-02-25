using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public abstract class PaginatedQueryDto
    {
        private const int DefaultPage = 1;
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        [Range(1, int.MaxValue, ErrorMessage = "Page must be >= 1")]
        public int Page { get; set; } = DefaultPage;

        [Range(1, MaxPageSize, ErrorMessage = "Page size must be >= 1")]
        public int PageSize { get; set; } = DefaultPageSize;

        public int PageSave => Page < 1 ? DefaultPage : Page;
        public int PageSizeSave => PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);
    }

    public class PaginatedMetaDto(int pageSize, int currentPage, int totalCount)
    {
        public int CurrentPage { get; init; } = currentPage;
        public int PageSize { get; init; } = pageSize;
        public int Total { get; init; } = totalCount;
        public int? NextPage { get; init; } = pageSize * currentPage < totalCount ? currentPage + 1 : null;
        public int? PrevPage { get; init; } = currentPage > 1 ? currentPage - 1 : null;
        public int LastPage { get; init; } =
            pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
    }

    public class PaginatedResponseDto<T>(List<T> data, int pageSize, int currentPage, int totalCount)
    {
        public List<T> Data { get; init; } = data;
        public PaginatedMetaDto Meta { get; init; } = new PaginatedMetaDto(pageSize, currentPage, totalCount);
    }
}