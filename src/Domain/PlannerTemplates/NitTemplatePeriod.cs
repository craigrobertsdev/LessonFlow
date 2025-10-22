using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

public class NitTemplatePeriod(int startPeriod, int numberOfPeriods) : PeriodTemplateBase(PeriodType.Nit, startPeriod, numberOfPeriods);