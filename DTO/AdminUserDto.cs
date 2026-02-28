namespace where_we_go.DTO;

using System.ComponentModel.DataAnnotations;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
}
public class AdminUpdateUserDto
{
    [Required]
    public required string Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, '.', '-', and '_' with no spaces.")]
    public required string userName { get; set; }

    [Required]
    [StringLength(50)]
    public required string Bio { get; set; }
    [Required]
    public required string ProfileUrl { get; set; }
}
