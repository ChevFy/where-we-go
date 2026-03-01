using where_we_go.Models;

namespace where_we_go.DTO
{
    public class UserResponseDto(User user, string[] role , string profileUrl)
    {
        public string Id { get; set; } = user.Id;
        public string? Email { get; set; } = user.Email;
        public string? UserName { get; set; } = user.UserName;
        public string Name { get; set; } = user.Name;
        public string? Bio { get; set; } = user.Bio;
        public string? ProfileUrl { get; set; } = profileUrl;
        public string[] Role { get; set; } = role;
    }
}