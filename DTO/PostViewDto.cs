using System.ComponentModel.DataAnnotations;

namespace where_we_go.Models;

public class PostDto
{
    [Required]
    public Guid PostId { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    public string LocationName { get; set; }
    public DateTime DateDeadline { get; set; }
    public string DateDeadlineFormatted => DateDeadline.ToString("dd/MM/yyyy");
    public string CategoryName { get; set; }
}