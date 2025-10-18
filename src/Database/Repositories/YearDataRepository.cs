using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class YearDataRepository(ApplicationDbContext context) : IYearDataRepository
{
    public void Add(YearData yearData) => context.YearData.Add(yearData);
    public async Task<YearData?> GetByUserIdAndYear(Guid userId, int calendarYear,
        CancellationToken cancellationToken)
    {
        return await context.YearData
            .Where(yd => yd.UserId == userId && yd.CalendarYear == calendarYear)
            .Include(yd => yd.SubjectsTaught)
            .Include(yd => yd.Students)
            .Include(yd => yd.WeekPlanners)
            .Include(yd => yd.LessonPlans)
            .Include(yd => yd.WeekPlannerTemplate)
            .ThenInclude(wp => wp.DayTemplates)
            .Include(yd => yd.WeekPlannerTemplate)
            .ThenInclude(wp => wp.DayTemplates)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<YearData?> GetById(YearDataId yearDataId, CancellationToken cancellationToken)
    {
        return await context.YearData
            .Where(yd => yd.Id == yearDataId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<YearLevelValue>> GetYearLevelsTaught(Guid UserId, int calendarYear,
        CancellationToken cancellationToken)
    {
        var yearData = await context.YearData
            .Where(yd => yd.CalendarYear == calendarYear)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return yearData?.YearLevelsTaught.ToList() ?? [];
    }

    public async Task<WeekPlannerTemplateId?> GetWeekPlannerTemplateId(YearDataId yearDataId, CancellationToken cancellationToken)
    {
        var yearData = await context.YearData
            .Where(yd => yd.Id == yearDataId)
            .Include(yd => yd.WeekPlannerTemplate)
            .FirstOrDefaultAsync(cancellationToken);

        if (yearData is null)
        {
            return null;
        }

        return yearData.WeekPlannerTemplate.Id;
    }
}