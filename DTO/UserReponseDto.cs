using where_we_go.Models;

namespace where_we_go.DTO
{
    public class UserResponseDto(User user)
    {
        public string Id { get; set; } = user.Id;
        public string? Email { get; set; } = user.Email;
        public string? UserName { get; set; } = user.UserName;
        public string Firstname { get; set; } = user.Firstname;
        public string Lastname { get; set; } = user.Lastname;
        public string? Bio { get; set; } = user.Bio;
        public string? ProfileUrl { get; set; } = user.ProfileUrl;
        public UserRoleEnum Role { get; set; } = user.Role;
    }
}