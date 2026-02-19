using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
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

        [Required]
        public required DateTime DateDeadline { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public required int MinParticipants { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public required int MaxParticipants { get; set; }
    }
}