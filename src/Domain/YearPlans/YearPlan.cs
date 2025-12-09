using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.YearPlans.DomainEvents;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Extensions;

namespace LessonFlow.Domain.YearPlans;

public class YearPlan : Entity<YearPlanId>, IAggregateRoot
{
    private Dictionary<(int term, int week), WeekPlanner> _weekPlannersByTermAndWeek = [];
    private Dictionary<DateOnly, WeekPlanner> _weekPlannersByWeekStart = [];
    public string SchoolName { get; private set; } = string.Empty;
    public WeekPlannerTemplate WeekPlannerTemplate { get; private set; }
    public WeekPlannerTemplateId WeekPlannerTemplateId { get; private set; }
    public Guid UserId { get; init; }
    public TermPlanner? TermPlanner { get; private set; }
    public int CalendarYear { get; init; }
    public List<Student> Students { get; private set; } = [];
    public List<YearLevel> YearLevelsTaught { get; private set; } = [];
    public List<Subject> SubjectsTaught { get; private set; } = [];
    public List<WeekPlanner> WeekPlanners { get; private set; } = [];
    public List<DayOfWeek> WorkingDays { get; private set; } = [];

    public void AddSubjects(List<Subject> subjects)
    {
        foreach (var subject in subjects)
        {
            if (!SubjectsTaught.Contains(subject))
            {
                SubjectsTaught.Add(subject);
            }
        }
    }

    public void AddStudents(List<Student> students)
    {
        foreach (var student in students)
        {
            AddStudent(student);
        }
    }

    public void AddStudent(Student student)
    {
        if (!Students.Contains(student))
        {
            Students.Add(student);
        }
    }

    public void AddYearLevel(YearLevel yearLevel)
    {
        if (!YearLevelsTaught.Contains(yearLevel))
        {
            YearLevelsTaught.Add(yearLevel);
        }
    }

    private bool NotInYearLevelsTaught(YearLevel yearLevel)
    {
        return YearLevelsTaught.Contains(yearLevel);
    }

    public void AddTermPlanner(TermPlanner termPlanner)
    {
        if (TermPlanner is not null)
        {
            throw new TermPlannerAlreadyAssociatedException();
        }

        TermPlanner = termPlanner;
    }

    public void AddYearLevelsTaught(List<YearLevel> yearLevelsTaught)
    {
        foreach (var yearLevel in yearLevelsTaught)
        {
            AddYearLevel(yearLevel);
        }
    }

    public void SetWeekPlannerTemplate(WeekPlannerTemplate weekPlannerTemplate)
    {
        WeekPlannerTemplate = weekPlannerTemplate;
        AddDomainEvent(new WeekPlannerTemplateAddedToYearPlanEvent(Guid.NewGuid(), WeekPlannerTemplate.Id));
    }

    public void UpdateWeekPlannerTemplate(WeekPlannerTemplate weekPlannerTemplate)
    {
        WeekPlannerTemplate.SetPeriods(weekPlannerTemplate.Periods);
        WeekPlannerTemplate.SetDayTemplates(weekPlannerTemplate.DayTemplates);
    }

    public void SetYearLevelsTaught(List<YearLevel> yearLevels)
    {
        yearLevels.Sort();
        YearLevelsTaught.Clear();
        YearLevelsTaught.AddRange(yearLevels);
    }

    public void AddWeekPlanner(WeekPlanner weekPlanner)
    {
        if (!WeekPlanners.Any(wp => wp.WeekStart == weekPlanner.WeekStart))
        {
            WeekPlanners.Add(weekPlanner);
            _weekPlannersByTermAndWeek.Add((weekPlanner.TermNumber, weekPlanner.WeekNumber), weekPlanner);
            _weekPlannersByWeekStart.Add(weekPlanner.WeekStart, weekPlanner);
            BuildDictionaries();
        }
        else
        {
            BuildDictionaries();
            var existingWeekPlanner = GetWeekPlanner(weekPlanner.TermNumber, weekPlanner.WeekNumber);
            existingWeekPlanner!.UpdateDayPlans(weekPlanner.DayPlans);
        }
    }

    public WeekPlanner? GetWeekPlanner(DateOnly weekStart)
    {
        if (_weekPlannersByWeekStart.Count == 0)
        {
            BuildDictionaries();
        }

        _weekPlannersByWeekStart.TryGetValue(weekStart, out var weekPlanner);
        return weekPlanner;
    }

    public WeekPlanner? GetWeekPlanner(int termNumber, int weekNumber)
    {
        if (_weekPlannersByTermAndWeek.Count == 0)
        {
            BuildDictionaries();
        }

        _weekPlannersByTermAndWeek.TryGetValue((termNumber, weekNumber), out var weekPlanner);
        return weekPlanner;
    }

    public DayPlan? GetDayPlan(DateOnly date)
    {
        var weekPlanner = GetWeekPlanner(date.GetWeekStart());
        if (weekPlanner is null)
        {
            return null;
        }

        return weekPlanner.DayPlans.First(dp => dp.Date == date); // Should never be null as WeekPlanner creates DayPlans for all days in its constructor
    }

    private void BuildDictionaries()
    {
        _weekPlannersByWeekStart = WeekPlanners.ToDictionary(wp => wp.WeekStart, wp => wp);
        _weekPlannersByTermAndWeek = WeekPlanners.ToDictionary(wp => (wp.TermNumber, wp.WeekNumber), wp => wp);
    }

    public YearPlan(Guid userId, WeekPlannerTemplate weekPlannerTemplate, string schoolName, int calendarYear)
    {
        Id = new YearPlanId(Guid.NewGuid());
        UserId = userId;
        SchoolName = schoolName;
        CalendarYear = calendarYear;
        WeekPlannerTemplate = weekPlannerTemplate;
        WeekPlannerTemplateId = weekPlannerTemplate.Id;

        BuildDictionaries();
    }

#pragma warning disable CS8618 // This function is called during database operation with WeekPlannerTemplate explicitly not set to prevent tracking issues.
    public YearPlan(Guid userId, AccountSetupState accountSetupState, List<Subject> subjects)
    {
        Id = new YearPlanId(Guid.NewGuid());
        UserId = userId;
        SubjectsTaught = subjects;
        SchoolName = accountSetupState.SchoolName;
        CalendarYear = accountSetupState.CalendarYear;
        WeekPlannerTemplateId = accountSetupState.WeekPlannerTemplate.Id;
        YearLevelsTaught = accountSetupState.YearLevelsTaught;
        YearLevelsTaught.Sort();
        WorkingDays = accountSetupState.WorkingDays;

        BuildDictionaries();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private YearPlan()
    {
        BuildDictionaries();
    }
}