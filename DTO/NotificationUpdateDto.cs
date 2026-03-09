using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class NotificationUpdateDto
    {
        [Required]
        public required Guid NotificationId { get; set; }

        [Required]
        public required bool IsRead { get; set; }
    }
}
