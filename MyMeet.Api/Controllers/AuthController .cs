using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMeet.Api.DTOs;
using MyMeet.Api.Exceptions;
using MyMeet.Api.Services;
using System.Security.Claims;

namespace MyMeet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
    {
        try
        {
            var jwt = await _authService.LoginWithGoogleAsync(dto.IdToken);
            return Ok(new { token = jwt });
        }
        catch (AuthException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var name = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (email is null)
            return Unauthorized(new { error = "Token did not contain a valid email claim." });

        return Ok(new { name, email, role });
    }
}