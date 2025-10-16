using System.Net;
using System.Text.Json;
using LessonFlow.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LessonFlow.Middleware;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        string type;
        if (ex is BaseException baseException)
        {
            context.Response.StatusCode = baseException.StatusCode;
            type = baseException.Type ?? "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        }

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = ex.Message,
            Status = context.Response.StatusCode,
            // Detail = " --- Make sure to change this in production! --- \n" + ex.Message
            Detail = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
                ? JsonSerializer.Serialize("Internal Server Error")
                : JsonSerializer.Serialize(ex.Message)
        };

        await context.Response.WriteAsync(problemDetails.Detail != null
            ? problemDetails.Detail!
            : "An internal server error has occurred");
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}