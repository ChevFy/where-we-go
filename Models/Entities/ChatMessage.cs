using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WhereWeGo.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        [Required]
        public required Guid MessageId {get; set;}

        [ForeignKey("GroupChatId")]
        public required Guid GroupChatId {get; set;}

        public virtual GroupChat GroupChat {get; set;} = null!;

        [ForeignKey("UserId")]
        public required string UserId {get; set;}

        public virtual User User {get; set;} = null!;

        [Required]
        public required string Message {get; set;}

        public DateTime SentAt {get; set;}

        public ChatMessage()
        {
            SentAt = DateTime.Now;
        }
    }
    
    
}