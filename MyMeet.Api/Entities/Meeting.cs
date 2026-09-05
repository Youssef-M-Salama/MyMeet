using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMeet.Api.Entities;

public class Meeting
{
    public Guid Id { get; set; }

    [Required]
    public Guid Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    public Guid HostUserId { get; set; }
    public User HostUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    [NotMapped]
    public bool IsActive => EndedAt == null;

    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
}