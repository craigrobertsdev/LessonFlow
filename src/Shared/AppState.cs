using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Exceptions;
using LessonFlow.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;

namespace LessonFlow.Shared;

public class AppState
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AppState> _logger;

    public AppState(AuthenticationStateProvider authStateProvider, IUserRepository userRepository, ILogger<AppState> logger)
    {
        _authStateProvider = authStateProvider;
        _userRepository = userRepository;
        _logger = logger;
    }

    public bool IsInitialised { get; private set; }
    public bool Initialising { get; private set; }

    private User? _user;
    public User? User
    {
        get => _user;
        set
        {
            _user = value;
            OnStateChanged?.Invoke();
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
        try
        {
            if (IsInitialised || Initialising) return;

            _logger.LogInformation("Starting AppState initialization");
            Initialising = true;
            OnStateChanged?.Invoke();

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated ?? false)
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
                    YearData.WeekPlannerTemplate.SortPeriods();
                }
                else
                {
                    user.AccountSetupState?.WeekPlannerTemplate.SortPeriods();
                }

                IsInitialised = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AppState");
            throw;
        }
        finally
        {
            Initialising = false;
            OnStateChanged?.Invoke();
        }
    }
}