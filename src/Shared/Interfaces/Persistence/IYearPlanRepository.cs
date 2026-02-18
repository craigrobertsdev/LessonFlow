using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IYearPlanRepository : IRepository<YearPlan>
{
    void Add(YearPlan yearPlan);
    Task<YearPlan?> GetByUserIdAndYear(Guid UserId, int calendarYear, CancellationToken ct);
    Task<YearPlan?> GetById(YearPlanId yearPlanId, CancellationToken ct);
    Task<WeekPlannerTemplateId?> GetWeekPlannerTemplateId(YearPlanId yearPlanId, CancellationToken ct);

    Task<List<YearLevel>> GetYearLevelsTaught(Guid UserId, int calendarYear,
        CancellationToken ct);

    Task<WeekPlanner> GetOrCreateWeekPlanner(YearPlanId yearPlanId, int year, int termNumber, int weekNumber,
        DateOnly weekStart,
        CancellationToken ct);

    Task<WeekPlanner?> GetWeekPlanner(YearPlanId yearPlanId, DateOnly weekStart, CancellationToken ct);
    Task UpdateTodoList(WeekPlannerId weekPlannerId, List<TodoItem> todos, CancellationToken ct);
}