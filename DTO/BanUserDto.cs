using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO;

public class BanUserDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 365)]
    public int DurationDays { get; set; }
}
