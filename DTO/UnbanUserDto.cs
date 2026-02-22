using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO;

public class UnbanUserDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
