using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.ValueObjects;

/// <summary>
///     Represents an entry in a day's schedule, which can be a lesson or a break.
/// </summary>
public class TemplatePeriod
{
    /// <summary>
    ///     Represents an entry in a day's schedule, which can be a lesson or a break.
    /// </summary>
    /// <param name="periodType">PeriodType.Lesson or PeriodType.Break</param>
    /// <param name="startPeriod">The period number in the day (1-based index)</param>
    /// <param name="name">"Recess" or "lunch" if PeriodType == PeriodType.Break</param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    public TemplatePeriod(PeriodType periodType, int startPeriod, string? name, TimeOnly startTime, TimeOnly endTime)
    {
        if (periodType is not PeriodType.Lesson and not PeriodType.Break)
        {
            throw new ArgumentException("Invalid period type");
        }

        PeriodType = periodType;
        StartPeriod = startPeriod;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
    }

    public PeriodType PeriodType { get; set; }
    public int StartPeriod { get; set; }
    public string? Name { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public static class TemplatePeriodExtensions
{
    public static List<TemplatePeriodDto> ToDtos(this IEnumerable<TemplatePeriod> periods)
    {
        var dtos = new List<TemplatePeriodDto>();
        foreach (var period in periods)
        {
            var dto = new TemplatePeriodDto
            (
                period.Name,
                period.StartPeriod,
                period.StartTime.ToString(),
                period.EndTime.ToString(),
                period.PeriodType == PeriodType.Break
            );

            dtos.Add(dto);
        }

        return dtos;
    }
}