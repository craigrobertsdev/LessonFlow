using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Exceptions;
using LessonFlow.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;

namespace LessonFlow.Shared;

public class AppState(AuthenticationStateProvider authStateProvider, IUserRepository userRepository, ILogger<AppState> logger)
{
    private bool _initialised;
    public bool Initialising { get; private set; } = true;
    private User? _user;
    public User? User
    {
        get => _user;
        set
        {
            _user = value;
            if (OnStateChanged is not null)
            {
                OnStateChanged?.Invoke();
            }
        }
    }

    private YearData? _yearData;
    public YearData? YearData
    {
        get => _yearData;
        set
        {
            _yearData = value;
            OnStateChanged?.Invoke();
        }
    }

    public event Action? OnStateChanged;

    public async Task InitialiseAsync()
    {
        logger.LogWarning($"AppState {GetHashCode()}, Initialised == {_initialised}");
        if (_initialised) return;

        _initialised = true;
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            try
            {
                var email = authState.User.Identity.Name!;
                var user = await userRepository.GetByEmail(email, CancellationToken.None);
                if (user is null)
                {
                    throw new UserNotFoundException();
                }

                User = user;
                if (user.AccountSetupComplete)
                {
                    var yearData = await userRepository.GetYearDataByYear(user.Id, user.LastSelectedYear, CancellationToken.None)
                        ?? throw new YearDataNotFoundException();
                    YearData = yearData;
                }
            }
            catch (Exception)
            {
                // Ignore exceptions here - user will be null
            }
            finally
            {
                Initialising = false;
                OnStateChanged?.Invoke();
            }
        }
    }
}
