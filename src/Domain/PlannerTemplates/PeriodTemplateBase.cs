using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Enums;
using LessonFlow.Shared.Interfaces;

namespace LessonFlow.Domain.PlannerTemplates;

public abstract class PeriodTemplateBase : IPlannerPeriod
{
    public PeriodType PeriodType { get; private set; }
    public int StartPeriod { get; set; }
    public int NumberOfPeriods { get; set; }

    protected PeriodTemplateBase(PeriodType periodType, int startPeriod, int numberOfPeriods)
    {
        PeriodType = periodType;
        StartPeriod = startPeriod;
        NumberOfPeriods = numberOfPeriods;
    }

    protected PeriodTemplateBase()
    {
    }

    public void SetNumberOfPeriods(int numberOfPeriods)
    {
        NumberOfPeriods = numberOfPeriods;
    }
}

public static class PeriodExtensions
{
    public static List<LessonTemplateDto> ToDtos(this IEnumerable<PeriodTemplateBase> periodTemplates)
    {
        return periodTemplates.Select(ls =>
            {
                return ls.PeriodType switch
                {
                    PeriodType.Lesson => new LessonTemplateDto(PeriodType.Lesson, ls.NumberOfPeriods, ls.StartPeriod,
                        ((LessonTemplate)ls).SubjectName,
                        null),
                    PeriodType.Break => new LessonTemplateDto(PeriodType.Break, ls.NumberOfPeriods, ls.StartPeriod,
                        null, ((BreakTemplate)ls).BreakDuty),
                    PeriodType.Nit => new LessonTemplateDto(PeriodType.Nit, ls.NumberOfPeriods, ls.StartPeriod, null,
                        null),
                    _ => throw new Exception($"{ls.PeriodType} is not a valid period type"),
                };
            })
            .ToList();
    }
}