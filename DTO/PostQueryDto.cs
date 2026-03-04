namespace where_we_go.DTO
{
    public class PostQueryDto : PaginatedQueryDto
    {
        public string? SortBy { get; set; }
        public string? NameFilter { get; set; }
        public List<Guid>? Categories { get; set; }
        public string? Status { get; set; }
    }
}