using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Exceptions;
using LessonFlow.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;

namespace LessonFlow.Shared;

public class AppState
{
    public AppState(AuthenticationStateProvider authStateProvider, IUserRepository userRepository, ILogger<AppState> logger)
    {
        _authStateProvider = authStateProvider;
        _userRepository = userRepository;
        _logger = logger;
    }

    public bool IsInitialised;
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
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AppState> _logger;

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
        if (IsInitialised) return;

        IsInitialised = true;
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            try
            {
                var email = authState.User.Identity.Name!;
                var user = await _userRepository.GetByEmail(email, CancellationToken.None);
                if (user is null)
                {
                    throw new UserNotFoundException();
                }

                User = user;
                if (user.AccountSetupComplete)
                {
                    var yearData = await _userRepository.GetYearDataByYear(user.Id, user.LastSelectedYear, CancellationToken.None)
                        ?? throw new YearDataNotFoundException();
                    YearData = yearData;
                }
                else
                {
                    user.AccountSetupState?.WeekPlannerTemplate.SortPeriods();
                }
            }
            catch (YearDataNotFoundException)
            {
                throw;
            }
            catch (UserNotFoundException)
            {
                throw;
            }
            finally
            {
                Initialising = false;
                OnStateChanged?.Invoke();
            }
        }
    }
}
