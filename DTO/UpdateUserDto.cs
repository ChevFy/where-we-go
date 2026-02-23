
using System.ComponentModel.DataAnnotations;
using where_we_go.Models;

namespace where_we_go.DTO
{
    public class UpdateUserDto
    {
        [Required]
        public string Id { get; set; } 

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; } 

         [Required]
        [StringLength(100, MinimumLength = 1)]
        public string? userName { get; set; } 

        [Required]
        [StringLength(50)]
        public string? Bio { get; set; }
        [Required]
        public string? ProfileUrl { get; set; }
    }
}