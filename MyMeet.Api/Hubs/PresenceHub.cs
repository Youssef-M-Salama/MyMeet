using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyMeet.Api.Services;
using System.Security.Claims;

namespace MyMeet.Api.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IMeetingService _meetingService;
    private readonly IConnectionTracker _tracker;

    public PresenceHub(IMeetingService meetingService, IConnectionTracker tracker)
    {
        _meetingService = meetingService;
        _tracker = tracker;
    }

    private Guid CurrentUserId =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task JoinMeeting(Guid meetingCode)
    {
        var userId = CurrentUserId;

        // Reuses the same durable REST logic from Phase 2 — writes JoinedAt to MySQL
        await _meetingService.JoinAsync(meetingCode, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, meetingCode.ToString());
        _tracker.Add(Context.ConnectionId, meetingCode, userId);

        await Clients.OthersInGroup(meetingCode.ToString())
            .SendAsync("UserJoined", new { userId, connectionId = Context.ConnectionId });
    }

    public async Task LeaveMeeting(Guid meetingCode)
    {
        var userId = CurrentUserId;

        await _meetingService.LeaveAsync(meetingCode, userId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingCode.ToString());
        _tracker.Remove(Context.ConnectionId);

        await Clients.OthersInGroup(meetingCode.ToString())
            .SendAsync("UserLeft", new { userId });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var entry = _tracker.Remove(Context.ConnectionId);

        if (entry is not null)
        {
            var (meetingCode, userId) = entry.Value;

            // Same durable write as an explicit Leave — covers tab close/crash/network drop
            await _meetingService.LeaveAsync(meetingCode, userId);

            await Clients.Group(meetingCode.ToString())
                .SendAsync("UserLeft", new { userId });
        }

        await base.OnDisconnectedAsync(exception);
    }
}