using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.Domain.WeekPlanners;

/// <summary>
///     Represents a week of planning for a teacher.
/// </summary>
public sealed class WeekPlanner : Entity<WeekPlannerId>, IAggregateRoot
{
    public YearData YearData { get; private set; }
    public DateOnly WeekStart { get; private set; }
    public int WeekNumber { get; private set; }
    public int TermNumber { get; private set; }
    public int Year { get; private set; }
    public List<DayPlan> DayPlans { get; private set; } = [];
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public void UpdateDayPlan(DayPlan dayPlan)
    {
        var idx = (int)dayPlan.DayOfWeek - 1;
        if (idx < 0 || idx >= 5)
        {
            throw new InvalidOperationException("DayPlan's date does not match this WeekPlanner.");
        }

        DayPlans[idx] = dayPlan;
    }

    public void SortDayPlans()
    {
        DayPlans.Sort((a, b) => a.DayOfWeek.CompareTo(b.DayOfWeek));
    }

    public WeekPlanner(
        YearData yearData,
        int year,
        int termNumber,
        int weekNumber,
        DateOnly weekStart)
    {
        Id = new WeekPlannerId(Guid.NewGuid());
        YearData = yearData;
        WeekStart = weekStart;
        WeekNumber = weekNumber;
        TermNumber = termNumber;
        Year = year;
        CreatedDateTime = DateTime.UtcNow;
        UpdatedDateTime = DateTime.UtcNow;

        for (var i = 0; i < 5; i++)
        {
            DayPlans.Add(new DayPlan(Id, weekStart.AddDays(i), [], []));
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private WeekPlanner() { }
}

public static class WeekPlannerExtensions
{
    public static List<DayTemplate> FromDtos(this IEnumerable<DayTemplateDto> dayTemplates)
    {
        return dayTemplates.Select(d => new DayTemplate(d.Templates.FromDtos(), d.DayOfWeek, d.Type)).ToList();
    }

    public static List<PeriodBase> FromDtos(this IEnumerable<LessonTemplateDto> lessonTemplates)
    {
        return lessonTemplates.Select(l =>
                (PeriodBase)(l.PeriodType switch
                {
                    PeriodType.Lesson => l.SubjectName is null
                        ? throw new ArgumentException("Lesson template does not have a SubjectName")
                        : new LessonPeriod(l.SubjectName, l.StartPeriod, l.NumberOfPeriods),
                    PeriodType.Break => new BreakPeriod(l.BreakDuty, l.StartPeriod, l.NumberOfPeriods),
                    PeriodType.Nit => new NitPeriod(l.StartPeriod, l.NumberOfPeriods),
                    _ => throw new Exception($"Unknown period type: {l.PeriodType}")
                }))
            .ToList();
    }

    public static WeekPlannerTemplate FromDto(this WeekPlannerTemplateDto weekPlannerTemplate, Guid userId)
    {
        return new WeekPlannerTemplate(weekPlannerTemplate.Periods.FromDtos(), weekPlannerTemplate.DayTemplates.FromDtos(), userId);
    }

    public static WeekPlannerDto ToDto(this WeekPlanner weekPlanner, WeekPlannerTemplate weekPlannerTemplate,
        List<Subject> subjects, List<Resource> resources)
    {
        return new WeekPlannerDto(
            weekPlanner.DayPlans.ToDtos(subjects, resources),
            weekPlannerTemplate.ToDto(),
            weekPlanner.WeekStart,
            weekPlanner.WeekNumber
        );
    }
}