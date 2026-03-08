
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using where_we_go.Models.Enums;

namespace where_we_go.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Required]
        public required Guid NotificationId { get; set; }

        [ForeignKey("UserId")]
        public required string UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [ForeignKey("PostId")]
        public required Guid PostId { get; set; }

        public virtual Post Post { get; set; } = null!;

        public string? Content { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsRead { get; set; }

        public NotificationType Type { get; set; }

        public Notification()
        {
            DateCreated = DateTime.Now;
            IsRead = false;
        }

    }

}