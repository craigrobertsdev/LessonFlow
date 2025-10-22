using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

public class BreakTemplate(string? breakDuty, int startPeriod, int numberOfPeriods)
    : PeriodTemplateBase(PeriodType.Break, startPeriod, numberOfPeriods)
{
    public string? BreakDuty { get; set; } = breakDuty;
}