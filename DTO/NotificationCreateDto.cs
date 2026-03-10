using System.ComponentModel.DataAnnotations;

using where_we_go.Models.Enums;

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

        [Required]
        public NotificationType Type { get; set; }

        [StringLength(200)]
        [Required]
        public required string Link { get; set; }
    }
}
