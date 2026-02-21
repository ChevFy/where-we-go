using System.ComponentModel.DataAnnotations;

namespace where_we_go.Models.DTOs.Mail
{
    public class SendMailRequestDto
    {
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsBodyHtml { get; set; } = true;
    }
}