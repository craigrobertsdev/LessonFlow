using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.PlannerTemplates;

/// <summary>
///     Represents a planner entry containing the subject and number of periods for a lesson.
/// </summary>
public class LessonStructure : PeriodBase
{
    public LessonStructure(string subjectName, int startPeriod, int numberOfPeriods)
        : base(PeriodType.Lesson, startPeriod, numberOfPeriods)
    {
        SubjectName = subjectName;
    }

    public string SubjectName { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private LessonStructure() : base()
    {
    }
}
