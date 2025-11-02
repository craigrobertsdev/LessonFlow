using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ITermPlannerRepository
{
    Task<TermPlanner?> GetById(TermPlannerId id, CancellationToken ct);

    Task<TermPlanner?> GetByYearPlanIdAndYear(YearPlanId yearPlanId, int calendarYear,
        CancellationToken ct);

    void Add(TermPlanner termPlanner);
    Task Delete(TermPlannerId id, CancellationToken ct);
}