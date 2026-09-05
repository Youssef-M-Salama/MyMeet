namespace MyMeet.Api.Exceptions;

public class MeetingException : Exception
{
    public int StatusCode { get; }

    public MeetingException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}