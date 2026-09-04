// IAuthService.cs
namespace MyMeet.Api.Services;

public interface IAuthService
{
    Task<string> LoginWithGoogleAsync(string googleIdToken);
}