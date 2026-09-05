namespace MyMeet.Api;

public record ParticipantDto(Guid UserId, string Name, string? ProfilePicture, DateTime JoinedAt, DateTime? LeftAt, bool IsActive);
