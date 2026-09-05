using MyMeet.Api.DTOs;

namespace MyMeet.Api.Services;

public interface IMeetingService
{
    Task<MeetingDto> CreateAsync(Guid hostUserId, CreateMeetingDto dto);
    Task<MeetingDto> GetByCodeAsync(Guid code);
    Task<MeetingDto> JoinAsync(Guid code, Guid userId);
    Task<MeetingDto> LeaveAsync(Guid code, Guid userId);
    Task<MeetingDto> EndAsync(Guid code, Guid userId);
}