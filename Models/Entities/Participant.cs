using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using where_we_go.Models.Enums;

namespace where_we_go.Models
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        [Required]
        public required Guid ParticipantId { get; set; }

        [ForeignKey("PostId")]
        public required Guid PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("UserId")]
        public required string UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public ParticipantStatus Status { get; set; } = ParticipantStatus.Pending;

        public DateTime DateJoin { get; set; } = DateTime.UtcNow;
    }
}