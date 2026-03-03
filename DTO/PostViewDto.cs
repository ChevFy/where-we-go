using System.ComponentModel.DataAnnotations;

namespace where_we_go.Models;

public class PostDto
{
    [Required]
    public Guid PostId { get; set; }
    [Required]
    public required string Title { get; set; }
    [Required]
    public required string Description { get; set; }
    public string? LocationName { get; set; }
    public DateTime DateDeadline { get; set; }

    public string? PostImgURL { get; set; }
    public string? DateDeadlineFormatted => DateDeadline.ToString("dd/MM/yyyy");
    public List<CategorySimpleDto> Categories { get; set; } = [];
}

public class CategorySimpleDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}