using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class NotificationCreateDto
    {
        [Required]
        public required string UserId { get; set; }

        [Required]
        public required Guid PostId { get; set; }

        [StringLength(500)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }
    }
}
