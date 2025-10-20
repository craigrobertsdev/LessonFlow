using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class WeekPlannerRepository(ApplicationDbContext context) : IWeekPlannerRepository
{
    public async Task<WeekPlanner?> GetWeekPlanner(YearDataId yearDataId, int year, int termNumber, int weekNumber,
        CancellationToken cancellationToken)
    {
        var weekPlanner = await context.WeekPlanners
            .Where(wp => wp.YearData.Id == yearDataId)
            .Where(wp => wp.WeekNumber == weekNumber)
            .Where(wp => wp.TermNumber == termNumber)
            .Where(wp => wp.Year == year)
            .Include(wp => wp.DayPlans)
            .ThenInclude(dp => dp.LessonPlans)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (weekPlanner is null) return null;

        weekPlanner.SortDayPlans();
        return weekPlanner;
    }

    public async Task<WeekPlanner?> GetWeekPlanner(YearDataId yearDataId, DateOnly weekStart, CancellationToken cancellationToken)
    {
        var weekPlanner = await context.WeekPlanners
            .Where(wp => wp.WeekStart == weekStart)
            .Include(wp => wp.DayPlans)
            .ThenInclude(dp => dp.LessonPlans)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (weekPlanner is null) return null;

        weekPlanner.SortDayPlans();
        return weekPlanner;
    }

    public async Task<WeekPlanner?> GetByLessonDate(DateOnly lessonDate, CancellationToken cancellationToken)
    {
        var weekPlanner = await context.WeekPlanners
            .Where(wp =>
                lessonDate.DayNumber - wp.WeekStart.DayNumber < 5 && lessonDate.DayNumber - wp.WeekStart.DayNumber >= 0)
            .FirstOrDefaultAsync(cancellationToken);

        return weekPlanner;
    }

    public async Task<WeekPlanner?> GetByYearAndWeekNumber(int year, int weekNumber,
        CancellationToken cancellationToken)
    {
        var weekPlanner = await context.WeekPlanners
            .Where(wp => wp.Year == year && wp.WeekNumber == weekNumber)
            .Include(wp => wp.DayPlans)
            .FirstOrDefaultAsync(cancellationToken);

        return weekPlanner;
    }

    public void Add(WeekPlanner weekPlanner)
    {
        context.WeekPlanners.Add(weekPlanner);
    }
}