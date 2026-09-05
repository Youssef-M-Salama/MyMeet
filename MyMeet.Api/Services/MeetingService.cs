using Microsoft.EntityFrameworkCore;
using MyMeet.Api.Data;
using MyMeet.Api.DTOs;
using MyMeet.Api.Entities;
using MyMeet.Api.Exceptions;

namespace MyMeet.Api.Services;

public class MeetingService : IMeetingService
{
    private readonly AppDbContext _db;

    public MeetingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MeetingDto> CreateAsync(Guid hostUserId, CreateMeetingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new MeetingException("Meeting title is required.", 400);

        var hostExists = await _db.Users.AnyAsync(u => u.Id == hostUserId);
        if (!hostExists)
            throw new MeetingException("Host user not found.", 404);

        var meeting = new Meeting
        {
            Title = dto.Title.Trim(),
            HostUserId = hostUserId,
            Code = await GenerateUniqueCodeAsync()
        };

        _db.Meetings.Add(meeting);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new MeetingException($"Failed to create meeting: {ex.InnerException?.Message ?? ex.Message}", 500);
        }

        return await GetByCodeAsync(meeting.Code);
    }

    public async Task<MeetingDto> GetByCodeAsync(Guid code)
    {
        var meeting = await _db.Meetings
            .Include(m => m.HostUser)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(m => m.Code == code);

        if (meeting is null)
            throw new MeetingException($"No meeting found with code '{code}'.", 404);

        return ToDto(meeting);
    }

    public async Task<MeetingDto> JoinAsync(Guid code, Guid userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Code == code);

        if (meeting is null)
            throw new MeetingException($"No meeting found with code '{code}'.", 404);

        if (!meeting.IsActive)
            throw new MeetingException("This meeting has already ended.", 400);

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new MeetingException("User not found.", 404);

        var participant = meeting.Participants.FirstOrDefault(p => p.UserId == userId);

        if (participant is null)
        {
            participant = new MeetingParticipant
            {
                MeetingId = meeting.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                LeftAt = null
            };
            _db.MeetingParticipants.Add(participant);
        }
        else if (participant.IsActive)
        {
            throw new MeetingException("User has already joined this meeting.", 400);
        }
        else
        {
            // Rejoining after having left
            participant.JoinedAt = DateTime.UtcNow;
            participant.LeftAt = null;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new MeetingException($"Failed to join meeting: {ex.InnerException?.Message ?? ex.Message}", 500);
        }

        return await GetByCodeAsync(code);
    }

    public async Task<MeetingDto> LeaveAsync(Guid code, Guid userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Code == code);

        if (meeting is null)
            throw new MeetingException($"No meeting found with code '{code}'.", 404);

        var participant = meeting.Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);

        if (participant is null)
            throw new MeetingException("User is not currently an active participant in this meeting.", 400);

        participant.LeftAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new MeetingException($"Failed to leave meeting: {ex.InnerException?.Message ?? ex.Message}", 500);
        }

        return await GetByCodeAsync(code);
    }

    private async Task<Guid> GenerateUniqueCodeAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = Guid.NewGuid();
            var exists = await _db.Meetings.AnyAsync(m => m.Code == candidate);
            if (!exists) return candidate;
        }

        throw new MeetingException("Failed to generate a unique meeting code after several attempts.", 500);
    }

    private static MeetingDto ToDto(Meeting m) => new(
        m.Id,
        m.Code,
        m.Title,
        m.HostUserId,
        m.HostUser.Name,
        m.CreatedAt,
        m.EndedAt,
        m.IsActive,
        m.Participants.Select(p => new ParticipantDto(
            p.UserId, p.User.Name, p.User.ProfilePicture, p.JoinedAt, p.LeftAt, p.IsActive
        )).ToList()
    );

   public async Task<MeetingDto> EndAsync(Guid code, Guid userId)
{
    var meeting = await _db.Meetings
        .Include(m => m.Participants)
            .ThenInclude(p => p.User)
        .Include(m => m.HostUser)
        .FirstOrDefaultAsync(m => m.Code == code);

    if (meeting is null)
        throw new MeetingException($"No meeting found with code '{code}'.", 404);

    if (meeting.HostUserId != userId)
        throw new MeetingException("Only the host can end this meeting.", 403);

    if (!meeting.IsActive)
        throw new MeetingException("This meeting has already ended.", 400);

    meeting.EndedAt = DateTime.UtcNow;

    try
    {
        await _db.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        throw new MeetingException($"Failed to end meeting: {ex.InnerException?.Message ?? ex.Message}", 500);
    }

    return ToDto(meeting);
}
}