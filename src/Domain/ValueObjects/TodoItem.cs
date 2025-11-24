using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.ValueObjects;

public class TodoItem
{
    public TodoItem(WeekPlannerId weekPlannerId, string text)
    {
        WeekPlannerId = weekPlannerId;
        Text = text;
    }
    public WeekPlannerId WeekPlannerId { get; set; }
    public string Text { get; set; }
    public bool IsComplete { get; set; }
    public bool IsMouseOver { get; set; }

    public void ToggleComplete() => IsComplete = !IsComplete;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TodoItem() { }
}
