using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.YearPlans;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IYearPlanRepository : IRepository<YearPlan>
{
    void Add(YearPlan yearPlan);
    Task<YearPlan?> GetByUserIdAndYear(Guid UserId, int calendarYear, CancellationToken cancellationToken);
    Task<YearPlan?> GetById(YearPlanId yearPlanId, CancellationToken cancellationToken);
    Task<WeekPlannerTemplateId?> GetWeekPlannerTemplateId(YearPlanId yearPlanId, CancellationToken cancellationToken);
    Task<List<YearLevelValue>> GetYearLevelsTaught(Guid UserId, int calendarYear,
        CancellationToken cancellationToken);
    Task<WeekPlanner> GetOrCreateWeekPlanner(YearPlanId yearPlanId, int year, int termNumber, int weekNumber, DateOnly weekStart, 
        CancellationToken cancellationToken);
    Task<WeekPlanner?> GetWeekPlanner(YearPlanId yearPlanId, DateOnly weekStart, CancellationToken cancellationToken);
    void Update(YearPlan yearPlan);
}