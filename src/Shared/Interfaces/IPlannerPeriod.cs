using LessonFlow.Domain.Enums;

namespace LessonFlow.Shared.Interfaces;

public interface IPlannerPeriod
{
    public int NumberOfPeriods { get; }
    public int StartPeriod { get; }
    public PeriodType PeriodType { get; }
    public bool LessonIsPlanned { get; }
}
