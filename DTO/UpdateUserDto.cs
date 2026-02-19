
using System.ComponentModel.DataAnnotations;
using where_we_go.Models;

namespace where_we_go.DTO
{
    public class UpdateUserDto(User user)
    {
        public string Id { get; set; } = user.Id;

        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; } = user.Name;
        [StringLength(150)]
        public string? Bio { get; set; } = user.Bio;
        public string? ProfileUrl { get; set; } = user.ProfileUrl;
    }
}