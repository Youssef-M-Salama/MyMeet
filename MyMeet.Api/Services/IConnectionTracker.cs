namespace MyMeet.Api.Services;

public interface IConnectionTracker
{
    void Add(string connectionId, Guid meetingCode, Guid userId);
    (Guid meetingCode, Guid userId)? Remove(string connectionId);
}