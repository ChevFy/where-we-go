using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace where_we_go.Models
{
    [Table("Users")]
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string Firstname { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string Lastname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public override string? Email { get; set; }

        [Required]
        [StringLength(256)]
        public override string? UserName { get; set; }

        public string? Bio { get; set; }
        public string? ProfileUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public UserRoleEnum Role { get; set; }
        public string? OAuthId { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();


        public User()
        {
            ProfileUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
            DateCreated = DateTime.Now;
            DateUpdated = DateTime.Now;
            Role = UserRoleEnum.User;
        }
    }
}