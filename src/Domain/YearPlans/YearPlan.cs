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
using System.Reflection.Metadata.Ecma335;

namespace LessonFlow.Domain.YearPlans;

public class YearPlan : Entity<YearPlanId>, IAggregateRoot
{
    private readonly Dictionary<(int term, int week), WeekPlanner> _weekPlannersByTermAndWeek = default!;
    private readonly Dictionary<DateOnly, WeekPlanner> _weekPlannersByWeekStart = default!;
    public string SchoolName { get; private set; } = string.Empty;
    public WeekPlannerTemplate WeekPlannerTemplate { get; set; }
    public Guid UserId { get; init; }
    public TermPlanner? TermPlanner { get; private set; }
    public int CalendarYear { get; init; }
    public List<Student> Students { get; private set; } = [];
    public List<YearLevelValue> YearLevelsTaught { get; private set; } = [];
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

    public void AddYearLevel(YearLevelValue yearLevel)
    {
        if (!YearLevelsTaught.Contains(yearLevel))
        {
            YearLevelsTaught.Add(yearLevel);
        }
    }

    private bool NotInYearLevelsTaught(YearLevelValue yearLevel)
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

    public void AddYearLevelsTaught(List<YearLevelValue> yearLevelsTaught)
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

    public void SetYearLevelsTaught(List<YearLevelValue> yearLevels)
    {
        yearLevels.Sort();
        YearLevelsTaught.Clear();
        YearLevelsTaught.AddRange(yearLevels);
    }

    public void AddWeekPlanner(WeekPlanner weekPlanner)
    {
        if (_weekPlannersByTermAndWeek.TryAdd((weekPlanner.TermNumber, weekPlanner.WeekNumber), weekPlanner) && _weekPlannersByWeekStart.TryAdd(weekPlanner.WeekStart, weekPlanner))
        {
            WeekPlanners.Add(weekPlanner);
        }
    }

    public WeekPlanner? GetWeekPlanner(DateOnly weekStart)
    {
        _weekPlannersByWeekStart.TryGetValue(weekStart, out var weekPlanner);
        return weekPlanner;
    }

    public WeekPlanner? GetWeekPlanner(int termNumber, int weekNumber)
    {
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

    public YearPlan(Guid userId, WeekPlannerTemplate weekPlannerTemplate, string schoolName,
        int calendarYear)
    {
        Id = new YearPlanId(Guid.NewGuid());
        UserId = userId;
        SchoolName = schoolName;
        CalendarYear = calendarYear;
        WeekPlannerTemplate = weekPlannerTemplate;

        _weekPlannersByWeekStart ??= WeekPlanners.ToDictionary(wp => wp.WeekStart, wp => wp);
        _weekPlannersByTermAndWeek ??= WeekPlanners.ToDictionary(wp => (wp.TermNumber, wp.WeekNumber), wp => wp);
    }

    public YearPlan(Guid userId, AccountSetupState accountSetupState)
    {
        Id = new YearPlanId(Guid.NewGuid());
        UserId = userId;
        SchoolName = accountSetupState.SchoolName;
        CalendarYear = accountSetupState.CalendarYear;
        WeekPlannerTemplate = accountSetupState.WeekPlannerTemplate;
        YearLevelsTaught = accountSetupState.YearLevelsTaught;
        YearLevelsTaught.Sort();
        WorkingDays = accountSetupState.WorkingDays;

        _weekPlannersByWeekStart ??= WeekPlanners.ToDictionary(wp => wp.WeekStart, wp => wp);
        _weekPlannersByTermAndWeek ??= WeekPlanners.ToDictionary(wp => (wp.TermNumber, wp.WeekNumber), wp => wp);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private YearPlan()
    {
        _weekPlannersByWeekStart ??= WeekPlanners.ToDictionary(wp => wp.WeekStart, wp => wp);
        _weekPlannersByTermAndWeek ??= WeekPlanners.ToDictionary(wp => (wp.TermNumber, wp.WeekNumber), wp => wp);
    }
}