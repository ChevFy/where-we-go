
using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }
        [StringLength(150)]
        public string? Bio { get; set; }
        public string? ProfileUrl { get; set; }
    }
}