using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

public class BreakPeriod(string? breakDuty, int startPeriod, int numberOfPeriods)
    : PeriodBase(PeriodType.Break, startPeriod, numberOfPeriods)
{
    public string? BreakDuty { get; set; } = breakDuty;
}