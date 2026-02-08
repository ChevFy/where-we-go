
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WhereWeGo.Models
{
    [Table("GroupChats")]
    public class GroupChat
    {
        [Key]
        [Required]
        public required Guid GroupChatId {get; set;}

        [ForeignKey("PostId")]
        public required Guid PostId {get; set;}

        public virtual Post Post {get; set;} = null!;

        public required string GroupChatName {get; set;}

        public virtual ICollection<User> Users  {get; set;} = new List<User>();
        

    }

}