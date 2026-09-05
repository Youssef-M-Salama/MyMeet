using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyMeet.Api.Data;
using MyMeet.Api.Entities;
using MyMeet.Api.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyMeet.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string> LoginWithGoogleAsync(string googleIdToken)
    {
        if (string.IsNullOrWhiteSpace(googleIdToken))
            throw new AuthException("Google ID token is missing or empty.", 400);

        var clientId = _config["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new AuthException("Server misconfiguration: Google:ClientId is not set.", 500);

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });
        }
        catch (InvalidJwtException ex)
        {
            // Covers: expired token, bad signature, wrong audience, malformed token
            throw new AuthException($"Google token validation failed: {ex.Message}", 401);
        }

        if (payload.EmailVerified != true)
            throw new AuthException("Google account email is not verified.", 401);

        if (string.IsNullOrWhiteSpace(payload.Email))
            throw new AuthException("Google token did not include an email address.", 400);

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Provider == "Google" && u.ProviderUserId == payload.Subject);

        if (user is null)
        {
            user = new User
            {
                Provider = "Google",
                ProviderUserId = payload.Subject,
                Name = payload.Name ?? "Unknown",
                Email = payload.Email,
                ProfilePicture = payload.Picture
            };


            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Covers: unique index violation (race condition on duplicate Provider+ProviderUserId), DB connection issues
                throw new AuthException($"Failed to save new user: {ex.InnerException?.Message ?? ex.Message}", 500);
            }
        }

        return GenerateJwt(user);
    }

    private string GenerateJwt(User user)
    {
        var jwtKey = _config["Jwt:Key"];
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new AuthException("Server misconfiguration: Jwt:Key is missing or too short (needs 32+ chars).", 500);

        if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
            throw new AuthException("Server misconfiguration: Jwt:Issuer or Jwt:Audience is not set.", 500);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // was JwtRegisteredClaimNames.Sub
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );


        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}