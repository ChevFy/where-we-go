
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WhereWeGo.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Required]
        public required Guid NotificationId {get; set;}
        
        [ForeignKey("UserId")]
        public required string UserId {get; set;}

        public virtual User User {get; set;} = null!;
        
        [ForeignKey("PostId")]
        public required string PostId {get; set;}
        
        public virtual Post Post {get; set;} = null!;

        public string? Content {get; set;}

        public DateTime DateCreated {get; set;}

        public bool isRead {get; set;}

        public string? Type {get; set;}

        public Notification()
        {
            DateCreated = DateTime.Now ;
            isRead = false;
            Type = null;
        }
        
    }
    
}