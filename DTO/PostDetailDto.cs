using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class PostDetailDto
    {
        [Required]
        public Guid PostId { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Title must be at least 2 characters")]
        [StringLength(100)]
        [Display(Name = "Post Title")]
        public required string Title { get; set; }

        [StringLength(200, ErrorMessage = "Description must be at most 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Location")]
        public required string LocationName { get; set; }

        [Required]
        public DateTime DateDeadline { get; set; }

        [Display(Name = "Deadline")]
        public string DateDeadlineFormatted => DateDeadline.ToString("dd MMM yyyy");

        [Display(Name = "Current Participants")]
        public int CurrentParticipants { get; set; }

        [Required]
        [Range(2, 100, ErrorMessage = "Max participants must be between 2 and 100.")]
        [Display(Name = "Max Participants")]
        public int MaxParticipants { get; set; }

        [Required]
        [Display(Name = "Category")]
        public required string CategoryName { get; set; }

        public string? PostImgURL {get; set;}

        [Required]
        public required string UserId { get; set; }
        public bool IsJoined { get; set; }
        public bool IsPending { get; set; }
    }
}