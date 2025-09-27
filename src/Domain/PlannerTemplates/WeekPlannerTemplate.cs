using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Domain.PlannerTemplates;

/// <summary>
///     This is used as a template for creating a WeekPlanner in the UI. If there are no lessons planned for a given
///     periods, the WeekPlanner will use the WeekPlannerTemplate to determine what to display.
///     Manages the number of periods in a week, which includes lessons and breaks.
///     A YearData object will have a single WeekPlannerTemplate object.
/// </summary>
public class WeekPlannerTemplate : Entity<WeekPlannerTemplateId>
{
    private readonly List<DayTemplate> _dayTemplates = [];
    private readonly List<TemplatePeriod> _periods = [];

    public Guid UserId { get; init; }
    public int NumberOfPeriods => Periods.Count;

    /// <summary>
    ///     The number of lessons and breaks in a day, ordered by their start time.
    /// </summary>
    public List<TemplatePeriod> Periods => _periods;

    /// <summary>
    ///     The planned lessons for each day. This is used to fill in the gaps in the WeekPlanner.
    ///     If the teacher has a different lesson planned for a given day, the WeekPlanner will use that instead.
    /// </summary>
    public List<DayTemplate> DayTemplates => _dayTemplates;

    public void SetPeriods(IEnumerable<TemplatePeriod> periods)
    {
        _periods.Clear();
        _periods.AddRange(periods);
    }

    public void SortPeriods()
    {
        _periods.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
    }

    public void SetDayTemplates(IReadOnlyList<DayTemplate> dayTemplates)
    {
        _dayTemplates.Clear();
        for (var i = 0; i < dayTemplates.Count; i++)
        {
            _dayTemplates.Insert(i, dayTemplates[i]);
        }
    }

    public WeekPlannerTemplate(List<TemplatePeriod> periods, List<DayTemplate> dayTemplates,
        Guid userId)
    {
        Id = new WeekPlannerTemplateId(Guid.NewGuid());
        _periods = periods;
        _dayTemplates = dayTemplates;
        UserId = userId;
    }

    public WeekPlannerTemplate(List<TemplatePeriod> periods, Guid userId)
    {
        Id = new WeekPlannerTemplateId(Guid.NewGuid());
        _periods = periods;
        UserId = userId;
    }

    public WeekPlannerTemplate(Guid userId)
    {
        Id = new WeekPlannerTemplateId(Guid.NewGuid());
        UserId = userId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public WeekPlannerTemplate()
    {
    }
}

public static class WeekPlannerTemplateExtensions
{
    public static WeekPlannerTemplateDto ToDto(this WeekPlannerTemplate weekPlannerTemplate)
    {
        return new WeekPlannerTemplateDto(
            weekPlannerTemplate.Periods.ToDtos(),
            weekPlannerTemplate.DayTemplates.ToDtos());
    }
}