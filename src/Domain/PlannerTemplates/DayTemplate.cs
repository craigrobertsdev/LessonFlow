using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

/// <summary>
///     Holds the structure of a day, including the period and their order.
///     This will be associated with many day plans and be used as the base to record
///     which subjects are being taught on a given day and how many period they take.
///     These will be read secondarily to the actual lesson plans to fill in the gaps prior to a lesson being planned.
/// </summary>
public class DayTemplate
{
    private readonly List<PeriodBase> _periods = [];

    public DayOfWeek DayOfWeek { get; private set; }
    public DayType Type { get; private set; }
    public bool IsWorkingDay => Type == DayType.WorkingDay;
    public List<PeriodBase> Periods => _periods;

    public void SetPeriods(IEnumerable<PeriodBase> periods)
    {
        _periods.Clear();
        _periods.AddRange(periods);
    }

    public void RemovePeriods(IEnumerable<PeriodBase> periods)
    {
        foreach (var period in periods)
            _periods.Remove(period);
    }

    public void AddPeriod(PeriodBase period)
    {
        _periods.Add(period);
        _periods.Sort((a, b) => a.StartPeriod.CompareTo(b.StartPeriod));
    }

    public DayTemplate(List<PeriodBase> periods, DayOfWeek dayOfWeek, DayType type)
    {
        _periods = periods;
        DayOfWeek = dayOfWeek;
        Type = type;
    }

    private DayTemplate()
    {
    }
}

public static class DayTemplateExtensions
{
    public static List<DayTemplateDto> ToDtos(this IEnumerable<DayTemplate> dayTemplates)
    {
        return dayTemplates.Select(dt => new DayTemplateDto(dt.DayOfWeek, dt.Type, dt.Periods.ToDtos())).ToList();
    }
}