
using System.ComponentModel.DataAnnotations;

using where_we_go.Models;

namespace where_we_go.DTO
{
    public class SelfUpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, '.', '-', and '_' with no spaces.")]
        public required string userName { get; set; }

        [StringLength(50)]
        public required string Bio { get; set; }
        [Required]
        public required string ProfileUrl { get; set; }
    }
}