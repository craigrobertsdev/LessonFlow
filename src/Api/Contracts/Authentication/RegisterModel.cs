namespace LessonFlow.Api.Contracts.Authentication;

public record RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmedPassword { get; set; } = string.Empty;
}