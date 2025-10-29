using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ILessonPlanRepository
{
    void Add(LessonPlan lesson);
    Task<List<LessonPlan>?> GetLessonsByDayPlanId(DayPlanId dayPlanId, CancellationToken cancellationToken);
    Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken cancellationToken);
    Task UpdateResources(LessonPlan lessonPlan, CancellationToken cancellationToken);
    Task<LessonPlan?> GetByDateAndPeriodStart(DayPlanId dayPlanId, DateOnly date, int period, CancellationToken cancellationToken);
    Task<List<LessonPlan>> GetByDayPlanAndDate(DayPlanId dayPlanId, DateOnly date,
        CancellationToken cancellationToken);

    Task<List<LessonPlan>> GetByDate(DayPlanId dayPlanId, DateOnly date, CancellationToken cancellationToken);
    bool UpdateLessonPlan(LessonPlan lessonPlan);
    void DeleteLessonPlans(IEnumerable<LessonPlan> lessonPlans);
    Task<LessonPlan?> GetByDayPlanAndDateAndPeriod(DayPlanId dayPlanId, DateOnly date, int period,
        CancellationToken cancellationToken);
}