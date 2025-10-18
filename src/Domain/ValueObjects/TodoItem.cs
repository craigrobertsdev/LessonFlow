using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.ValueObjects;

public class TodoItem(LessonPlanId lessonPlanId, string text)
{
    public LessonPlanId LessonPlanId { get; set; } = lessonPlanId;
    public string Text { get; set; } = text;
    public bool IsComplete { get; set; }
}
