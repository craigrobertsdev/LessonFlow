namespace LessonFlow.Api.Features.Auth;

public static class Register
{
    public static async Task<IResult> Endpoint(Request request)
    {
        return TypedResults.Ok();
    }

    public record Request(string Email, string Password, string ConfirmPassword);
}