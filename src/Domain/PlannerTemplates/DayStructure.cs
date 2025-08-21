using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

/// <summary>
///     Holds the structure of a day, including the periods and their order.
///     This will be associated with many day plans and be used as the base to record
///     which subjects are being taught on a given day and how many periods they take.
///     These will be read secondarily to the actual lesson plans to fill in the gaps prior to a lesson being planned.
/// </summary>
public class DayStructure
{
    private readonly List<PeriodBase> _periods = [];

    public DayOfWeek DayOfWeek { get; private set; }
    public DayType Type { get; private set; }
    public IReadOnlyList<PeriodBase> Periods => _periods.AsReadOnly();

    public DayStructure(List<PeriodBase> periods, DayOfWeek dayOfWeek, DayType type)
    {
        _periods = periods;
        DayOfWeek = dayOfWeek;
        Type = type;
    }

    private DayStructure()
    {
    }
}

public static class DayTemplateExtensions
{
    public static List<DayTemplateDto> ToDtos(this IEnumerable<DayStructure> dayTemplates)
    {
        return dayTemplates.Select(dt => new DayTemplateDto(dt.DayOfWeek, dt.Type, dt.Periods.ToDtos())).ToList();
    }
}