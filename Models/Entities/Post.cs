
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace WhereWeGo.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        public Guid PostId {get; set;}
        [Required]
        public required string UserId {get ; set;}

        [ForeignKey("UserId")]
        public User User {get; set;} = null!;

        public virtual ICollection<Category> Categories {get; set;} = new List<Category>();

        
        [Required]
        public required string Title {get; set;}

        [Required]
        public required string Description {get; set;}

        [Required]
        public required int MinParticipants {get; set;}

        [Required]
        public required int MaxParticipants {get; set;}

        public int CurrentParticipants {get; set;}

        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
 
        public DateTime DateCreated {get; set;}

        public DateTime DateDeadline {get; set;}

        public string Status {get; set;}
        
        [Required]
        public required string LocationName {get; set;}

        public float? LocationLat {get; set;}

        public float? LocationLon {get; set;}
        
        public string? PostUrl { get; set; }
        
        [Required]
        public required string InviteCode {get; set;}

        public virtual ICollection<Notification> Notifications {get; set;} = new List<Notification>();


        public Post()
        {
            Status = "ACTIVE";
            DateCreated = DateTime.Now;
            DateDeadline = DateTime.Now;
            PostUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTiyTHhPsApqZdEyUNhkGYS40BBOU8Oeb1vgw&s";
            InviteCode = null! ; // ต้องทำ service generate code
        }
    }
    
}