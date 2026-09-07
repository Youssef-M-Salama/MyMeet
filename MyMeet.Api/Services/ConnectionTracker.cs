using System.Collections.Concurrent;

namespace MyMeet.Api.Services;

public class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, (Guid meetingCode, Guid userId)> _connections = new();

    public void Add(string connectionId, Guid meetingCode, Guid userId)
        => _connections[connectionId] = (meetingCode, userId);

    public (Guid meetingCode, Guid userId)? Remove(string connectionId)
        => _connections.TryRemove(connectionId, out var value) ? value : null;
}