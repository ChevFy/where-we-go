using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    [FutureDateTime]
    public class PostCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public required string Description { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
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
        [Range(1, int.MaxValue)]
        public required int MinParticipants { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [MinMaxValidation("MinParticipants", "MaxParticipants")]
        public required int MaxParticipants { get; set; }

        public string? PostImgkey { get; set; }

        public List<Guid>? CategoryIds { get; set; }
    }
}