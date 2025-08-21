using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Extensions;

namespace LessonFlow.Domain.WeekPlanners;

/// <summary>
///     Represents the actually planned lessons and events for a given day.
/// </summary>
public class DayPlan : Entity<DayPlanId>
{
    private readonly List<LessonPlan> _lessonPlans = [];
    private readonly List<SchoolEvent> _schoolEvents = [];
    private Dictionary<int, string>? _breakDutyOverrides;

    public WeekPlannerId WeekPlannerId { get; private set; }
    public DateOnly Date { get; private set; }
    public DayOfWeek DayOfWeek => Date.DayOfWeek;
    public IReadOnlyList<LessonPlan> LessonPlans => _lessonPlans.AsReadOnly();
    public IReadOnlyList<SchoolEvent> SchoolEvents => _schoolEvents.AsReadOnly();
    public IReadOnlyDictionary<int, string>? BreakDutyOverrides => _breakDutyOverrides?.AsReadOnly();

    public void AddLessonPlan(LessonPlan lessonPlan)
    {
        if (_lessonPlans.Contains(lessonPlan))
        {
            return;
        }

        _lessonPlans.Add(lessonPlan);
    }

    public void SetBreakDutyOverride(int periodNumber, string dutyName)
    {
        if (_breakDutyOverrides is not null && string.IsNullOrWhiteSpace(dutyName))
        {
            _breakDutyOverrides.Remove(periodNumber);
        }
        else
        {
            _breakDutyOverrides ??= new Dictionary<int, string>();
            _breakDutyOverrides[periodNumber] = dutyName;
        }
    }

    public void RemoveBreakDutyOverride(int periodNumber)
    {
        _breakDutyOverrides?.Remove(periodNumber);
    }

    public string? GetBreakDutyOverride(int periodNumber)
    {
        return _breakDutyOverrides?.GetValueOrDefault(periodNumber);
    }

    public DayPlan(WeekPlannerId weekPlannerId, DateOnly date, List<LessonPlan> lessonPlans, List<SchoolEvent>? schoolEvents)
    {
        Id = new DayPlanId(Guid.NewGuid());
        _lessonPlans = lessonPlans;
        WeekPlannerId = weekPlannerId;
        Date = date;

        if (schoolEvents is not null)
        {
            _schoolEvents = schoolEvents;
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private DayPlan() {}
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