namespace LessonFlow.Api.Features.Auth;

public static class GetCurrentUser
{
    public static IResult Endpoint(HttpContext context)
    {
        var user = context.User.Identity?.Name;
        if (user == null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(new { Email = user });
    }
}