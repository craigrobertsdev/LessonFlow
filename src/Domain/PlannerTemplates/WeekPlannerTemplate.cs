using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Enums;
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
    public Guid UserId { get; init; }
    public int NumberOfPeriods => Periods.Count;

    /// <summary>
    ///     The number of lessons and breaks in a day, ordered by their start time.
    /// </summary>
    public List<TemplatePeriod> Periods { get; private set; } = [];

    /// <summary>
    ///     The planned lessons for each day. This is used to fill in the gaps in the WeekPlanner.
    ///     If the teacher has a different lesson planned for a given day, the WeekPlanner will use that instead.
    /// </summary>
    public List<DayTemplate> DayTemplates { get; private set; } = [];

    public void SetPeriods(IEnumerable<TemplatePeriod> periods)
    {
        Periods.Clear();
        Periods.AddRange(periods);
    }

    public void SetDayTemplates(IReadOnlyList<DayTemplate> dayTemplates)
    {
        DayTemplates.Clear();
        for (var i = 0; i < dayTemplates.Count; i++)
        {
            DayTemplates.Insert(i, dayTemplates[i]);
        }
    }

    public void AddPeriod(TemplatePeriod period)
    {
        Periods.Add(period);
        foreach (var dayTemplate in DayTemplates)
        {
            if (!dayTemplate.IsWorkingDay) continue;

            var newPeriod = new LessonTemplate(string.Empty, dayTemplate.Periods[^1].StartPeriod + 1, 1);
            dayTemplate.AddPeriod(newPeriod);
        }
    }

    public void RemovePeriod(TemplatePeriod period)
    {
        Periods.Remove(period);
        foreach (var dayTemplate in DayTemplates)
        {
            if (!dayTemplate.IsWorkingDay) continue;

            var idx = dayTemplate.Periods.FindIndex(p => p.StartPeriod == period.StartPeriod);
            dayTemplate.Periods.RemoveAt(idx);
        }
    }

    public void UpdatePeriod(TemplatePeriod period)
    {
        var existing = Periods.FindIndex(p => p.StartPeriod == period.StartPeriod);
        if (existing > -1) Periods[existing] = period;

        foreach (var dayTemplate in DayTemplates)
        {
            if (!dayTemplate.IsWorkingDay) continue;
            var idx = dayTemplate.Periods.FindIndex(p => p.StartPeriod == period.StartPeriod);
            if (idx >= 0)
            {
                var existingPeriod = dayTemplate.Periods[idx];
                if (existingPeriod.PeriodType != period.PeriodType)
                {
                    PeriodTemplateBase newPeriod;
                    if (period.PeriodType == PeriodType.Break)
                    {
                        newPeriod = new BreakTemplate(period.Name ?? "Break", existingPeriod.StartPeriod, 1);
                    }
                    else
                    {
                        newPeriod = new LessonTemplate(string.Empty, existingPeriod.StartPeriod, 1);
                    }
                    dayTemplate.Periods[idx] = newPeriod;
                }
            }
        }
    }

    public PeriodTemplateBase? GetTemplatePeriod(DayOfWeek dayOfWeek, int startPeriod)
    {
        var dayTemplate = DayTemplates.Find(dt => dt.DayOfWeek == dayOfWeek);
        if (dayTemplate == null || !dayTemplate.IsWorkingDay) return null;

        return dayTemplate.Periods.Find(p => p.StartPeriod == startPeriod);
    }

    public void SortPeriods()
    {
        Periods.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        DayTemplates.Sort((a, b) => a.DayOfWeek.CompareTo(b.DayOfWeek));
        DayTemplates.ForEach(dt => dt.Periods.Sort((a, b) => a.StartPeriod.CompareTo(b.StartPeriod)));
    }

    public int GetLessonPeriodCount(int startPeriod)
    {
        return Periods.Count(p => p.StartPeriod >= startPeriod && p.PeriodType == PeriodType.Lesson);
    }

    public WeekPlannerTemplate(Guid userId, List<TemplatePeriod> periods,
        List<DayTemplate> dayTemplates)
    {
        Id = new WeekPlannerTemplateId(Guid.NewGuid());
        Periods = periods;
        DayTemplates = dayTemplates;
        UserId = userId;
    }

    public WeekPlannerTemplate(Guid userId, List<TemplatePeriod> periods)
    {
        Id = new WeekPlannerTemplateId(Guid.NewGuid());
        Periods = periods;
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