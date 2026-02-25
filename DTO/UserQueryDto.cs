namespace where_we_go.DTO
{
    public class UserQueryDto : PaginatedQueryDto
    {
        public string? SortBy { get; set; }
        public string? NameFilter { get; set; }
    }
}