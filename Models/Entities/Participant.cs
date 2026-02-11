
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WhereWeGo.Models
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        [Required]
        public required Guid ParticipantId {get; set;}
        
        public required string UserId {get ; set;}

        [ForeignKey("UserId")]
        public virtual User User {get; set;} = null!;

        [ForeignKey("PostId")]
        public required Guid PostId {get; set;}

        public Post Post {get; set;} = null!;
        
        public required DateTime DateJoin {get; set;} 

        public required string status {get; set;}

        public Participant()
        {
            DateJoin = DateTime.Now;
            status = ParticipantStatus.Pending;
        }
        
        
    }
    
}