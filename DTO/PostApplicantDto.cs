using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class PostApplicantDto
    {
        [Required]
        public required Guid PostId { get; set; }

        [Required]
        public required string[] ApplicantIds { get; set; }
    }
}