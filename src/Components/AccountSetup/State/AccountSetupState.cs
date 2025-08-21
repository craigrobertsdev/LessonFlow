using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Components.AccountSetup.State;

public class AccountSetupState
{
    public Guid Id { get; init; }
    public event Action? OnChange;
    public event Action<Pages.AccountSetup.ChangeDirection>? OnDirectionChange;

    public AccountSetupStep CurrentStep { get; private set; } = AccountSetupStep.BasicInfo;

    public readonly List<AccountSetupStep> StepOrder =
    [
        AccountSetupStep.BasicInfo, AccountSetupStep.Subjects, AccountSetupStep.Timing,
        AccountSetupStep.Schedule
    ];

    public HashSet<AccountSetupStep> CompletedSteps { get; private set; } = [];

    public string SchoolName { get; private set; } = string.Empty;
    public int CalendarYear { get; private set; } = DateTime.Now.Year;
    public List<YearLevelValue> YearLevelsTaught { get; private set; } = [];
    public List<string> SubjectsTaught { get; private set; } = [];
    public List<DayOfWeek> WorkingDays { get; private set; } =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];
    public List<DayColumn> ScheduleGrid { get; private set; } = [];
    public ScheduleConfig ScheduleConfig { get; private set; } = new()
    {
        NumberOfLessons = 6,
        NumberOfBreaks = 2,
        StartTime = new TimeOnly(9, 10, 0),
        EndTime = new TimeOnly(15, 10, 0),
        ScheduleSlots =
        [
            new ScheduleSlot
            {
                Id = 1, PeriodType = PeriodType.Lesson, Name = "Lesson 1",
                StartTime = new TimeOnly(09, 10, 0),
                EndTime = new TimeOnly(10, 00, 0)
            },
            new ScheduleSlot
            {
                Id = 2, PeriodType = PeriodType.Lesson, Name = "Lesson 2",
                StartTime = new TimeOnly(10, 00, 0),
                EndTime = new TimeOnly(10, 50, 0)
            },
            new ScheduleSlot
            {
                Id = 3, PeriodType = PeriodType.Break, Name = "Recess",
                StartTime = new TimeOnly(10, 50, 0),
                EndTime = new TimeOnly(11, 20, 0)
            },
            new ScheduleSlot
            {
                Id = 4, PeriodType = PeriodType.Lesson, Name = "Lesson 3",
                StartTime = new TimeOnly(11, 20, 0),
                EndTime = new TimeOnly(12, 10, 0)
            },
            new ScheduleSlot
            {
                Id = 5, PeriodType = PeriodType.Lesson, Name = "Lesson 4",
                StartTime = new TimeOnly(12, 10, 0),
                EndTime = new TimeOnly(13, 00, 0)
            },
            new ScheduleSlot
            {
                Id = 6, PeriodType = PeriodType.Break, Name = "Lunch",
                StartTime = new TimeOnly(13, 0, 0),
                EndTime = new TimeOnly(13, 30, 0)
            },
            new ScheduleSlot
            {
                Id = 7, PeriodType = PeriodType.Lesson, Name = "Lesson 5",
                StartTime = new TimeOnly(13, 30, 0),
                EndTime = new TimeOnly(14, 20, 0)
            },
            new ScheduleSlot
            {
                Id = 8, PeriodType = PeriodType.Lesson, Name = "Lesson 6",
                StartTime = new TimeOnly(14, 20, 0),
                EndTime = new TimeOnly(15, 10, 0)
            }
        ]
    };

    public string? Error { get; private set; }
    public bool IsLoading { get; private set; }

    public void SetCurrentStep(AccountSetupStep step)
    {
        CurrentStep = step;
        NotifyStateChanged();
    }

    public void UpdateStep(AccountSetupStep step, Pages.AccountSetup.ChangeDirection direction)
    {
        CompletedSteps.Add(step);
        CurrentStep = step;
        NotifyStateChanged(direction);
    }

    public bool IsNextStep(AccountSetupStep step)
    {
        return StepOrder.IndexOf(step) == StepOrder.IndexOf(CurrentStep) + 1;
    }

    public AccountSetupStep GetLastCompletedStep() => CompletedSteps.LastOrDefault();
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

    public void SetYearLevelsTaught(List<YearLevelValue> levels)
    {
        YearLevelsTaught = levels;
        NotifyStateChanged();
    }

    public void SetSubjectsTaught(List<string> subjects)
    {
        SubjectsTaught = subjects;
        NotifyStateChanged();
    }

    public void SetWorkingDays(List<DayOfWeek> days)
    {
        WorkingDays = days;
        NotifyStateChanged();
    }

    public void SetScheduleGrid(List<DayColumn> grid)
    {
        ScheduleGrid = grid;
        NotifyStateChanged();
    }

    public void SetScheduleConfig(ScheduleConfig config)
    {
        ScheduleConfig = config;
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
    private void NotifyStateChanged(Pages.AccountSetup.ChangeDirection direction) => OnDirectionChange?.Invoke(direction);
}

[Owned]
public class ScheduleConfig
{
    public int NumberOfLessons { get; set; }
    public int NumberOfBreaks { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<ScheduleSlot> ScheduleSlots { get; set; } = [];
}

[Owned]
public class ScheduleSlot
{
    public int Id { get; init; }
    public PeriodType PeriodType { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public Subject? Subject { get; set; }
}

[Owned]
public class DayColumn
{

    public DayOfWeek DayOfWeek { get; init; }
    public bool IsWorkingDay { get; set; }
    public List<ScheduleSlot> ScheduleSlots { get; set; } = [];
}