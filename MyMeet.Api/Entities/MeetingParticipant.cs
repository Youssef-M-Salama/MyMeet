using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMeet.Api.Entities;

public class MeetingParticipant
{
    public Guid Id { get; set; }

    [Required]
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    [NotMapped]
    public bool IsActive => LeftAt == null;
}