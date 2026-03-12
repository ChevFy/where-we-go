

namespace where_we_go.DTO
{
    public class AdminPostDto
    {
        public Guid PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public DateTime DateDeadline { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string LocationName { get; set; } = string.Empty;
    }

    public class AdminPostUpdateDto
    {
        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public List<Guid> CategoryIds { get; set; } = [];
    }
}
