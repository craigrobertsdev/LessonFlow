using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Shared.Interfaces.Services;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Exceptions;

namespace LessonFlow.Components.Pages;

public partial class AccountSetup : ComponentBase, IDisposable
{
    [Inject] public AppState AppState { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] ICurriculumService CurriculumService { get; set; } = null!;
    [Inject] IUserRepository UserRepository { get; set; } = null!;
    [Inject] ILogger<AccountSetup> Logger { get; set; } = null!;

    public AccountSetupState AccountSetupState { get; set; } = null!;
    private AccountSetupStep _accountSetupStep { get; set; } = default!;
    private List<string> _subjectNames = [];
    private bool _initialLoading = true;
    private bool _redirectToLogin = false;
    private bool _redirectToWeekPlanner = false;
    private bool _hasInitialized = false;

    private User User => AppState.User!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await AppState.InitialiseAsync();
        }
        catch (UserNotFoundException)
        {
            NavigationManager.NavigateTo("/Account/Login", true);
        }

        if (User.AccountSetupComplete)
        {
            _redirectToWeekPlanner = true;
            NavigationManager.NavigateTo("/WeekPlanner", new NavigationOptions
            {
                ReplaceHistoryEntry = true
            });
        }

        if (User.AccountSetupState is null)
        {
            User.AccountSetupState = new(User.Id);
        }

        AccountSetupState = User.AccountSetupState!;
        AccountSetupState.OnDirectionChange += ChangeAccountSetupStep;

        _accountSetupStep = User.AccountSetupState!.CurrentStep;

        try
        {
            AccountSetupState.SetLoading(true);
            _subjectNames = CurriculumService.GetSubjectNames();
        }
        catch (Exception ex)
        {
            AccountSetupState.SetError($"Error loading subjects: {ex.Message}");
        }
        finally
        {
            AccountSetupState.SetLoading(false);
            _initialLoading = false;
        }
    }

    protected override void OnInitialized()
    {
        if (_hasInitialized || User is null)
        {
            return;
        }

        _hasInitialized = true;

    }

    private async Task SaveAccountSetupState()
    {
        try
        {
            if (User is null) throw new UserNotFoundException();
            await UserRepository.UpdateAccountSetupState(User.Id, AccountSetupState);
            User.AccountSetupState = AccountSetupState;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving account setup state");
            AccountSetupState.SetError("An error occurred while saving your account setup. Please try again.");
        }
    }

    private async void ChangeAccountSetupStep(ChangeDirection direction)
    {
        var idx = AccountSetupState.StepOrder.IndexOf(_accountSetupStep);
        if (idx == 0 && direction == ChangeDirection.Back || idx == AccountSetupState.StepOrder.Count - 1 && direction == ChangeDirection.Forward)
        {
            return;
        }

        _accountSetupStep = direction == ChangeDirection.Back ? AccountSetupState.StepOrder[idx - 1] : AccountSetupState.StepOrder[idx + 1];

        await UserRepository.UpdateAccountSetupState(User.Id, AccountSetupState);

        StateHasChanged();
    }

    private async Task CompleteAccountSetup()
    {
        if (AppState.User is null)
        {
            _redirectToLogin = true;
            NavigationManager.NavigateTo("/auth/login", true);
            return;
        }

        var yearData = new YearData(User.Id, AccountSetupState);

        await UserRepository.CompleteAccountSetup(AppState.User.Id, yearData);
        AppState.YearDataByYear.Add(yearData.CalendarYear, yearData);
    }

    public void Dispose()
    {
        if (AccountSetupState is not null)
        {
            AccountSetupState.OnDirectionChange -= ChangeAccountSetupStep;
        }
    }
}
