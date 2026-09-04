namespace MyMeet.Api.Exceptions;

public class AuthException : Exception
{
    public int StatusCode { get; }

    public AuthException(string message, int statusCode = 401) : base(message)
    {
        StatusCode = statusCode;
    }
}