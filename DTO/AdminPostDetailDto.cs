using where_we_go.Models;

namespace where_we_go.DTO
{
    public class AdminPostDetailDto
    {
        public Guid PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public int MinParticipants { get; set; }
        public DateTime DateDeadline { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public float? LocationLat { get; set; }
        public float? LocationLon { get; set; }
        public string? PostImageKey { get; set; }
        public string? InviteCode { get; set; }
        public List<CategorySimpleDto> Categories { get; set; } = [];
        public List<ParticipantInfoDto> Participants { get; set; } = [];
    }

    public class ParticipantInfoDto
    {
        public Guid ParticipantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateJoin { get; set; }
    }
}
