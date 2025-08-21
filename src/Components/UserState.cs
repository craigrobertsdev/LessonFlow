using LessonFlow.Domain.Users;
using LessonFlow.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;

namespace LessonFlow.Components;

public class UserState(
    IServiceScopeFactory scopeFactory,
    AuthenticationStateProvider authenticationStateProvider)
{
    public User? User { get; private set; }

    public async Task<User?> EnsureUserLoaded(CancellationToken cancellationToken = default)
    {
        if (User is not null)
        {
            return User;
        }

        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal.Identity is { IsAuthenticated: true })
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            User = await repo.GetByEmail(principal.Identity!.Name!, CancellationToken.None);
        }

        return User;
    }
}