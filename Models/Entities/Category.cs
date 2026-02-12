
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace where_we_go.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Required]
        public required Guid CategoryId {get; set;}

        [Required]
        public required string Name {get; set;}

        public string? Description {get; set;}

        public ICollection<Post> Posts { get; set; } = new List<Post>();

    }
    
}