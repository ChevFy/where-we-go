namespace where_we_go.DTO
{
    public class NotificationQueryDto : PaginatedQueryDto
    {
        // <summary>
        // status available: latest, oldest
        // </summary>
        public string? SortBy { get; set; }
        public bool? IsReadFilter { get; set; }
        public Guid? PostIdFilter { get; set; }
    }
}
