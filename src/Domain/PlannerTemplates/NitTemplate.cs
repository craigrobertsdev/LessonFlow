using LessonFlow.Domain.Enums;
using LessonFlow.Shared.Interfaces;

namespace LessonFlow.Domain.PlannerTemplates;

public class NitTemplate(int startPeriod, int numberOfPeriods) : PeriodTemplateBase(PeriodType.Nit, startPeriod, numberOfPeriods), ILessonPeriod
{
    public string SubjectName => "NIT";
}