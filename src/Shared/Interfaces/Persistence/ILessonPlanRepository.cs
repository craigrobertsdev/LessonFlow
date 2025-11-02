using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ILessonPlanRepository
{
    void Add(LessonPlan lesson);
    Task<List<LessonPlan>?> GetLessonsByDayPlanId(DayPlanId dayPlanId, CancellationToken ct);
    Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken ct);
    Task UpdateResources(LessonPlan lessonPlan, CancellationToken ct);
    Task<LessonPlan?> GetByDateAndPeriodStart(DayPlanId dayPlanId, DateOnly date, int period, CancellationToken ct);
    Task<List<LessonPlan>> GetByDayPlanAndDate(DayPlanId dayPlanId, DateOnly date,
        CancellationToken ct);

    Task<List<LessonPlan>> GetByDate(DayPlanId dayPlanId, DateOnly date, CancellationToken ct);
    Task<bool> UpdateLessonPlan(LessonPlan lessonPlan, CancellationToken ct);
    void DeleteLessonPlans(IEnumerable<LessonPlan> lessonPlans);
    Task<LessonPlan?> GetByDayPlanAndDateAndPeriod(DayPlanId dayPlanId, DateOnly date, int period,
        CancellationToken ct);
}