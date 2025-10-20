using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ILessonPlanRepository
{
    void Add(LessonPlan lesson);
    Task<List<LessonPlan>?> GetLessonsByYearDataId(YearDataId yearDataId, CancellationToken cancellationToken);
    Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken cancellationToken);
    Task<LessonPlan?> GetByDateAndPeriodStart(YearDataId yearDataId, DateOnly date, int period, CancellationToken cancellationToken);
    Task<List<LessonPlan>> GetByYearDataAndDate(YearDataId yearDataId, DateOnly date,
        CancellationToken cancellationToken);

    Task<List<LessonPlan>> GetByDate(YearDataId yearDataId, DateOnly date, CancellationToken cancellationToken);
    Task AddLessonPlan(LessonPlan lessonPlan, CancellationToken cancellationToken);
    bool UpdateLessonPlan(LessonPlan lessonPlan);
    void DeleteLessonPlans(IEnumerable<LessonPlan> lessonPlans);

    Task<LessonPlan?> GetByYearDataAndDateAndPeriod(YearDataId yearDataId, DateOnly date, int period,
        CancellationToken cancellationToken);
}