using System.Security.Claims;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Extensions;

public static class AuthExtensions
{
    public static UserId GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (userId is null)
        {
            throw new InvalidOperationException("TeacherId claim not found");
        }

        return new UserId(Guid.Parse(userId));
    }
}