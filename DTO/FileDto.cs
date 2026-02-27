using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class FileUploadDto
    {
        public required string ObjectName { get; set; }

        public required IFormFile File { get; set; }
    }

    public class FileDownloadDto
    {
        [Required]
        public required string ObjectName { get; set; }

        [Required]
        [Range(1, 60 * 60 * 24)]
        public int ExpiryInSeconds { get; set; } = 3600;
    }
}