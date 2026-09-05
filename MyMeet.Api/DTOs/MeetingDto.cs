namespace MyMeet.Api;

public record MeetingDto(
    Guid Id,
    Guid Code,
    string Title,
    Guid HostUserId,
    string HostName,
    DateTime CreatedAt,
    DateTime? EndedAt,
    bool IsActive,
    List<ParticipantDto> Participants
);
