using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

public abstract class PeriodBase
{
    public PeriodType PeriodType { get; private set; }
    public int StartPeriod { get; set; }
    public int NumberOfPeriods { get; set; }

    protected PeriodBase(PeriodType periodType, int startPeriod, int numberOfPeriods)
    {
        PeriodType = periodType;
        StartPeriod = startPeriod;
        NumberOfPeriods = numberOfPeriods;
    }

    protected PeriodBase()
    {
    }

    public void SetNumberOfPeriods(int numberOfPeriods)
    {
        NumberOfPeriods = numberOfPeriods;
    }
}

public static class PeriodExtensions
{
    public static List<LessonTemplateDto> ToDtos(this IEnumerable<PeriodBase> periodTemplates)
    {
        return periodTemplates.Select(ls =>
            {
                return ls.PeriodType switch
                {
                    PeriodType.Lesson => new LessonTemplateDto(PeriodType.Lesson, ls.NumberOfPeriods, ls.StartPeriod,
                        ((LessonPeriod)ls).SubjectName,
                        null),
                    PeriodType.Break => new LessonTemplateDto(PeriodType.Break, ls.NumberOfPeriods, ls.StartPeriod,
                        null, ((BreakPeriod)ls).BreakDuty),
                    PeriodType.Nit => new LessonTemplateDto(PeriodType.Nit, ls.NumberOfPeriods, ls.StartPeriod, null,
                        null),
                    _ => throw new Exception($"{ls.PeriodType} is not a valid period type"),
                };
            })
            .ToList();
    }
}