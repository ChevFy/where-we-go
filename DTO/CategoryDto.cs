using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
