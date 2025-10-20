namespace LessonFlow.Shared.Exceptions;

public abstract class BaseException(string message, int statusCode, string type) : Exception(message)
{
    public int StatusCode { get; set; } = statusCode;
    public string Type { get; set; } = type;
}