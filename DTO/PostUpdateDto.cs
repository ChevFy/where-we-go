using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    [FutureDateTime]
    [EventAfterDeadline]
    public class PostUpdateDto
    {
        [Required]
        public required Guid PostId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public required string Description { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public required string LocationName { get; set; }

        [StringLength(200, MinimumLength = 1)]
        public string? LocationLat { get; set; }

        public string? LocationLon { get; set; }

        [Required]
        public required DateTime DateDeadline { get; set; }

        [Required]
        public List<CategoryDetailDto> Categories { get; set; } = [];

        [Required]
        public required TimeOnly TimeDeadline { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeOnly EventTime { get; set; }

        [Required]
        [Range(2, int.MaxValue, ErrorMessage = "Min participants must be at least 2 (including you as owner).")]
        public required int MinParticipants { get; set; }

        [Required]
        [Range(2, int.MaxValue, ErrorMessage = "Max people must be at least 2 (including you as owner).")]
        [MinMaxValidation("MinParticipants", "MaxParticipants")]
        public required int MaxParticipants { get; set; }

        public string? PostImgkey { get; set; }

        public List<Guid>? CategoryIds { get; set; }
    }
}
