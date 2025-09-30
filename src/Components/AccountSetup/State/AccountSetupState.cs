using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Components.AccountSetup.State;

public class AccountSetupState
{
    public Guid Id { get; init; }
    public event Action? OnChange;
    public event Action<ChangeDirection>? OnDirectionChange;

    public AccountSetupStep CurrentStep { get; private set; } = AccountSetupStep.BasicInfo;

    public readonly List<AccountSetupStep> StepOrder =
    [
        AccountSetupStep.BasicInfo, AccountSetupStep.Subjects, AccountSetupStep.Timing,
        AccountSetupStep.Schedule
    ];

    public List<AccountSetupStep> CompletedSteps { get; private set; } = [];
    public string SchoolName { get; private set; } = string.Empty;
    public int CalendarYear { get; private set; } = DateTime.Now.Year;
    public List<YearLevelValue> YearLevelsTaught { get; private set; } = [];
    public List<string> SubjectsTaught { get; private set; } = [];
    public List<DayOfWeek> WorkingDays { get; private set; } =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public WeekPlannerTemplate WeekPlannerTemplate { get; set; }
    public string? Error { get; private set; }
    public bool IsLoading { get; private set; }

    public void UpdateStep(AccountSetupStep step, ChangeDirection direction)
    {
        if (CurrentStep == step) return;
        if (!CompletedSteps.Contains(step))
        {
            CompletedSteps.Add(step);
        }
        CurrentStep = step;
        NotifyStateChanged(direction);
    }

    public void SetSchoolName(string schoolName)
    {
        SchoolName = schoolName;
        NotifyStateChanged();
    }

    public void SetCalendarYear(int year)
    {
        CalendarYear = year;
        NotifyStateChanged();
    }

    public void SetSubjectsTaught(List<string> subjects)
    {
        SubjectsTaught = subjects;
        NotifyStateChanged();
    }

    public void UpdateTiming(TimeOnly startTime, TimeOnly endTime, List<TemplatePeriod> periods)
    {
        StartTime = startTime;
        EndTime = endTime;
        WeekPlannerTemplate.SetPeriods(periods);
        
        NotifyStateChanged();
    }

    public void SetError(string error)
    {
        Error = error;
        NotifyStateChanged();
    }

    public void ClearError()
    {
        Error = null;
        NotifyStateChanged();
    }

    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
    private void NotifyStateChanged(ChangeDirection direction) => OnDirectionChange?.Invoke(direction);

    public AccountSetupState(Guid userId)
    {
        List<TemplatePeriod> periods = [

            new TemplatePeriod
            (
                PeriodType.Lesson, 1, "Lesson 1",
                new TimeOnly(09, 10, 0),
                new TimeOnly(10, 00, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Lesson, 2, "Lesson 2",
                new TimeOnly(10, 00, 0),
                new TimeOnly(10, 50, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Break, 3, "Recess",
                new TimeOnly(10, 50, 0),
                new TimeOnly(11, 20, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Lesson, 4, "Lesson 3",
                new TimeOnly(11, 20, 0),
                new TimeOnly(12, 10, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Lesson, 5, "Lesson 4",
                new TimeOnly(12, 10, 0),
                new TimeOnly(13, 00, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Break, 6, "Lunch",
                new TimeOnly(13, 0, 0),
                new TimeOnly(13, 30, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Lesson, 7, "Lesson 5",
                new TimeOnly(13, 30, 0),
                new TimeOnly(14, 20, 0)
            ),
            new TemplatePeriod
            (
                PeriodType.Lesson, 8, "Lesson 6",
                new TimeOnly(14, 20, 0),
                new TimeOnly(15, 10, 0)
            )
            ];

        WeekPlannerTemplate = new WeekPlannerTemplate(periods, userId);
        StartTime = new TimeOnly(9, 10);
        EndTime = new TimeOnly(15, 10);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AccountSetupState() { }
}
