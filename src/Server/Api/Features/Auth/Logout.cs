namespace LessonFlow.Api.Features.Auth;

public static class Logout
{
    public static IResult Endpoint(HttpContext context)
    {
        return TypedResults.SignOut();
    }
}