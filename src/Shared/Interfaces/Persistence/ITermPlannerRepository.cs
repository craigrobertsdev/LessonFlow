using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners;

namespace LessonFlow.Interfaces.Persistence;

public interface ITermPlannerRepository
{
    Task<TermPlanner?> GetById(TermPlannerId id, CancellationToken cancellationToken);

    Task<TermPlanner?> GetByYearDataIdAndYear(YearDataId yearDataId, int calendarYear,
        CancellationToken cancellationToken);

    void Add(TermPlanner termPlanner);
    Task Delete(TermPlannerId id, CancellationToken cancellationToken);
}