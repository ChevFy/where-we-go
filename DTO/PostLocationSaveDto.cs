using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class PostLocationSaveDto
    {
        [Required]
        public required string PostId {get; set;}
        [Required]

        public required string LocationLat {get; set;}
        [Required]

        public required string LocationLon {get; set;}

    }
}