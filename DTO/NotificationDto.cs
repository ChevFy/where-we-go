using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class NotificationDto
    {
        [Required]
        public Guid NotificationId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        public Guid PostId { get; set; }

        public string? Content { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsRead { get; set; }

        public string? Type { get; set; }

        public string? DateCreatedFormatted
        {
            get
            {
                var thailandTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                var localTime = TimeZoneInfo.ConvertTime(DateCreated, thailandTz);
                return localTime.ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
