using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

public class NitPeriod(int startPeriod, int numberOfPeriods) : PeriodBase(PeriodType.Nit, startPeriod, numberOfPeriods);