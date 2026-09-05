using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMeet.Api.Exceptions;
using MyMeet.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyMeet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingsController(IMeetingService meetingService) => _meetingService = meetingService;

    private Guid CurrentUserId =>
    Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingDto dto)
    {
        try
        {
            var meeting = await _meetingService.CreateAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetByCode), new { code = meeting.Code }, meeting);
        }
        catch (MeetingException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(Guid code)
    {
        try
        {
            var meeting = await _meetingService.GetByCodeAsync(code);
            return Ok(meeting);
        }
        catch (MeetingException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(Guid code)
    {
        try
        {
            var meeting = await _meetingService.JoinAsync(code, CurrentUserId);
            return Ok(meeting);
        }
        catch (MeetingException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpPost("{code}/leave")]
    public async Task<IActionResult> Leave(Guid code)
    {
        try
        {
            var meeting = await _meetingService.LeaveAsync(code, CurrentUserId);
            return Ok(meeting);
        }
        catch (MeetingException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }
    [HttpPost("{code}/end")]
    public async Task<IActionResult> End(Guid code)
    {
        try
        {
            var meeting = await _meetingService.EndAsync(code, CurrentUserId);
            return Ok(meeting);
        }
        catch (MeetingException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }
}