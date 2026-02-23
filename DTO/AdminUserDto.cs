namespace where_we_go.DTO;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
}
