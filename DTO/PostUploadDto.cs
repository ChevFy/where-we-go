using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class PostUploadDto
    {
        [Required]
        public required string PostId {get; set;}
        public string? PostImageKey {get; set;}
    }
}