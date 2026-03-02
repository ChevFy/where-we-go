using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace where_we_go.Models
{
    [Table("Users")]
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public override string? Email { get; set; }

        [Required]
        [StringLength(256)]
        public override string? UserName { get; set; }

        public string? Bio { get; set; }
        public string? ProfileImageKey { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? OAuthId { get; set; }

        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        public string? BannedBy { get; set; }


        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();


        public User()
        {
            ProfileImageKey = null;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;
        }
    }
}