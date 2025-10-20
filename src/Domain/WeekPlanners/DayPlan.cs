using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Shared.Extensions;

namespace LessonFlow.Domain.WeekPlanners;

/// <summary>
///     Represents the actually planned lessons and events for a given day.
/// </summary>
public class DayPlan : Entity<DayPlanId>
{
    public WeekPlannerId WeekPlannerId { get; private set; }
    public DateOnly Date { get; private set; }
    public DayOfWeek DayOfWeek => Date.DayOfWeek;
    public List<LessonPlan> LessonPlans { get; set; } = [];
    public List<SchoolEvent> SchoolEvents { get; set; } = [];
    public Dictionary<int, string> BreakDutyOverrides { get; set; } = [];
    public string? BeforeSchoolDuty { get; set; } 
    public string? AfterSchoolDuty { get; set; } 

    public void AddLessonPlan(LessonPlan lessonPlan)
    {
        if (LessonPlans.Contains(lessonPlan))
        {
            return;
        }

        LessonPlans.Add(lessonPlan);
    }

    public void SetBreakDutyOverride(int periodNumber, string dutyName)
    {
        if (string.IsNullOrWhiteSpace(dutyName))
        {
            BreakDutyOverrides.Remove(periodNumber);
        }
        else if (BreakDutyOverrides.TryGetValue(periodNumber, out _))
        {
            BreakDutyOverrides[periodNumber] = dutyName;
        }
        else
        {
            BreakDutyOverrides.Add(periodNumber, dutyName);
        }
    }

    public void RemoveBreakDutyOverride(int periodNumber)
    {
        BreakDutyOverrides.Remove(periodNumber);
    }

    public string? GetBreakDutyOverride(int periodNumber)
    {
        return BreakDutyOverrides.GetValueOrDefault(periodNumber);
    }

    public DayPlan(WeekPlannerId weekPlannerId, DateOnly date, List<LessonPlan> lessonPlans, List<SchoolEvent>? schoolEvents)
    {
        Id = new DayPlanId(Guid.NewGuid());
        LessonPlans = lessonPlans;
        WeekPlannerId = weekPlannerId;
        Date = date;

        if (schoolEvents is not null)
        {
            SchoolEvents = schoolEvents;
        }
    }

    public DayPlan Clone()
    {
        return new DayPlan(WeekPlannerId, Date, [.. LessonPlans], [.. SchoolEvents])
        {
            BreakDutyOverrides = new Dictionary<int, string>(BreakDutyOverrides)
        };
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private DayPlan() { }
}

public static class DayPlanExtensions
{
    public static List<DayPlanDto> ToDtos(this IEnumerable<DayPlan> dayPlans, List<Subject> subjects,
        List<Resource> resources)
    {
        return dayPlans.Select(dp =>
            new DayPlanDto(
            dp.Date,
            dp.LessonPlans.ToDtos(resources, subjects),
            dp.SchoolEvents.ToDtos(),
            dp.BreakDutyOverrides?.Count > 0
                ? dp.BreakDutyOverrides.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : null
            )).ToList();
    }
}