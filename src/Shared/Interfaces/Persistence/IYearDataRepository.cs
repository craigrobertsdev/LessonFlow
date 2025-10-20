using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IYearDataRepository : IRepository<YearData>
{
    void Add(YearData yearData);
    Task<YearData?> GetByUserIdAndYear(Guid UserId, int calendarYear, CancellationToken cancellationToken);
    Task<YearData?> GetById(YearDataId yearDataId, CancellationToken cancellationToken);
    Task<WeekPlannerTemplateId?> GetWeekPlannerTemplateId(YearDataId yearDataId, CancellationToken cancellationToken);
    Task<List<YearLevelValue>> GetYearLevelsTaught(Guid UserId, int calendarYear,
        CancellationToken cancellationToken);
}