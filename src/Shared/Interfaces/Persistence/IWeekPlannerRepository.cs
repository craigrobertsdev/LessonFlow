using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.WeekPlanners;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IWeekPlannerRepository
{
    Task<WeekPlanner?> GetWeekPlanner(YearDataId yearDataId, int year, int termNumber, int weekNumber,
        CancellationToken cancellationToken);
    Task<WeekPlanner?> GetWeekPlanner(YearDataId yearDataId, DateOnly weekStart, CancellationToken cancellationToken);
    Task<WeekPlanner?> GetByLessonDate(DateOnly lessonDate, CancellationToken cancellationToken);
    Task<WeekPlanner?> GetByYearAndWeekNumber(int year, int weekNumber, CancellationToken cancellationToken);
    void Add(WeekPlanner weekPlanner);
}