using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;

namespace LessonFlow.Domain.YearPlans;

/// <summary>
///     Represents a week of planning for a teacher.
/// </summary>
public sealed class WeekPlanner : Entity<WeekPlannerId>, IAggregateRoot
{
    public YearPlanId YearPlanId { get; private set; }
    public DateOnly WeekStart { get; private set; }
    public int WeekNumber { get; private set; }
    public int TermNumber { get; private set; }
    public int Year { get; private set; }
    public bool HasLessonPlansLoaded { get; private set; }
    public List<DayPlan> DayPlans { get; private set; } = [];
    public List<TodoItem> Todos { get; private set; } = [];
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public void UpdateDayPlan(DayPlan dayPlan)
    {
        var idx = (int)dayPlan.DayOfWeek - 1;
        if (idx < 0 || idx >= 5)
        {
            throw new InvalidOperationException("DayPlan's date does not match this WeekPlanner.");
        }

        var existingDayPlan = DayPlans[idx];
        existingDayPlan.UpdateFrom(dayPlan);
    }

    public void UpdateDayPlans(List<DayPlan> dayPlans)
    {
        foreach (var dayPlan in dayPlans)
        {
            UpdateDayPlan(dayPlan);
        }
    }

    public DayPlan? GetDayPlan(DateOnly date)
    {
        return DayPlans.FirstOrDefault(d => d.Date == date);
    }

    public void SortDayPlans()
    {
        DayPlans.Sort((a, b) => a.DayOfWeek.CompareTo(b.DayOfWeek));
    }

    public void UpdateTodos(List<TodoItem> todos)
    {
        Todos = todos;
    }

    public void DeleteTodoItem(TodoItem todoItem)
    {
        Todos.Remove(todoItem);
    }

    public void AddTodoItem(string text)
    {
        var todoItem = new TodoItem(Id, text);
        Todos.Add(todoItem);
    }

    public WeekPlanner(
        YearPlanId yearPlanId,
        int year,
        int termNumber,
        int weekNumber,
        DateOnly weekStart)
    {
        Id = new WeekPlannerId(Guid.NewGuid());
        YearPlanId = yearPlanId;
        WeekStart = weekStart;
        WeekNumber = weekNumber;
        TermNumber = termNumber;
        Year = year;
        CreatedDateTime = DateTime.UtcNow;
        UpdatedDateTime = DateTime.UtcNow;

        for (var i = 0; i < 5; i++)
        {
            DayPlans.Add(new DayPlan(Id, weekStart.AddDays(i), [], []));
        }
    }

    public void SetLessonPlansLoaded(bool loaded)
    {
        HasLessonPlansLoaded = loaded;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private WeekPlanner() { }
}

public static class WeekPlannerExtensions
{
    public static List<DayTemplate> FromDtos(this IEnumerable<DayTemplateDto> dayTemplates)
    {
        return dayTemplates.Select(d => new DayTemplate(d.Templates.FromDtos(), d.DayOfWeek, d.Type)).ToList();
    }

    public static List<PeriodTemplateBase> FromDtos(this IEnumerable<LessonTemplateDto> lessonTemplates)
    {
        return lessonTemplates.Select(l =>
                (PeriodTemplateBase)(l.PeriodType switch
                {
                    PeriodType.Lesson => l.SubjectName is null
                        ? throw new ArgumentException("Lesson template does not have a SubjectName")
                        : new LessonTemplate(l.SubjectName, l.StartPeriod, l.NumberOfPeriods),
                    PeriodType.Break => new BreakTemplate(l.BreakDuty, l.StartPeriod, l.NumberOfPeriods),
                    PeriodType.Nit => new NitTemplate(l.StartPeriod, l.NumberOfPeriods),
                    _ => throw new Exception($"Unknown period type: {l.PeriodType}")
                }))
            .ToList();
    }

    public static WeekPlannerTemplate FromDto(this WeekPlannerTemplateDto weekPlannerTemplate, Guid userId)
    {
        return new WeekPlannerTemplate(userId, weekPlannerTemplate.Periods.FromDtos(), weekPlannerTemplate.DayTemplates.FromDtos());
    }

    public static WeekPlannerDto ToDto(this WeekPlanner weekPlanner, WeekPlannerTemplate weekPlannerTemplate,
        List<Subject> subjects, List<Resource> resources)
    {
        return new WeekPlannerDto(
            weekPlanner.DayPlans.ToDtos(subjects, resources),
            weekPlannerTemplate.ToDto(),
            weekPlanner.WeekStart,
            weekPlanner.WeekNumber
        );
    }
}