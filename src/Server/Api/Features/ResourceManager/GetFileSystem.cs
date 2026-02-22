namespace LessonFlow.Api.Features.ResourceManager;

public static class GetFileSystem
{
    public static async Task<IResult> Endpoint(HttpContext httpContext)
    {
    }

    public record Response(FileSystemDto FileSystem);
}