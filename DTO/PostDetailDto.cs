namespace where_we_go.DTO
{
    public class PostDetailDto
    {
        public Guid PostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string LocationName { get; set; }
        public string DateDeadlineFormatted { get; set; } // Formatted as "dd MMM yyyy"
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string CategoryName { get; set; }
    }
}