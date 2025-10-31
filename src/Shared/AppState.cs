using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace LessonFlow.Shared;

public class AppState
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AppState> _logger;
    private readonly ITermDatesService _termDatesService;

    public AppState(AuthenticationStateProvider authStateProvider, IUserRepository userRepository, ILogger<AppState> logger, ITermDatesService termDatesService)
    {
        _authStateProvider = authStateProvider;
        _userRepository = userRepository;
        _logger = logger;
        _termDatesService = termDatesService;
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

    private Dictionary<int, YearPlan> _yearPlanByYear = [];
    public Dictionary<int, YearPlan> YearPlanByYear
    {
        get => _yearPlanByYear;
        set
        {
            _yearPlanByYear = value;
            OnStateChanged?.Invoke();
        }
    }

    private int _currentYear = DateTime.Now.Year;
    public int CurrentYear
    {
        get => _currentYear;
        set
        {
            _currentYear = value;
            OnStateChanged?.Invoke();
        }
    }

    private int _currentTerm = 1;
    public int CurrentTerm {
        get => _currentTerm;
        set
        {
            _currentTerm = value;
            OnStateChanged?.Invoke();
        }
    }

    private int _currentWeek = 1;
    public int CurrentWeek {
        get => _currentWeek;
        set
        {
            _currentWeek = value;
            OnStateChanged?.Invoke();
        }
    }

    public YearPlan CurrentYearPlan => _yearPlanByYear[_currentYear];

    public event Action? OnStateChanged;

    public async Task InitialiseAsync()
    {
        try
        {
            if (IsInitialised || Initialising) return;

            _logger.LogInformation("Starting AppState initialization");
            Initialising = true;

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
                    var yearPlan = await _userRepository.GetYearPlanByYear(user.Id, user.LastSelectedYear, CancellationToken.None)
                        ?? throw new YearPlanNotFoundException();
                    yearPlan.WeekPlannerTemplate.SortPeriods();
                    YearPlanByYear.Add(yearPlan.CalendarYear, yearPlan);
                    CurrentYear = user.LastSelectedYear;
                    CurrentTerm = _termDatesService.GetTermNumber(DateOnly.FromDateTime(DateTime.Now));
                    CurrentWeek = _termDatesService.GetWeekNumber(DateOnly.FromDateTime(DateTime.Now));
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

    public void AddNewYearPlan(int year, YearPlan yearPlan)
    {
        if (!IsInitialised) return;

        _yearPlanByYear.Add(year, yearPlan);
        OnStateChanged?.Invoke();
    }
}