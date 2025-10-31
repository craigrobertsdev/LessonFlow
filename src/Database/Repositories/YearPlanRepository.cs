using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class YearPlanRepository(ApplicationDbContext context) : IYearPlanRepository
{
    public void Add(YearPlan yearPlan) => context.YearPlans.Add(yearPlan);
    public async Task<YearPlan?> GetByUserIdAndYear(Guid userId, int calendarYear,
        CancellationToken cancellationToken)
    {
        return await context.YearPlans
            .Where(yd => yd.UserId == userId && yd.CalendarYear == calendarYear)
            .Include(yd => yd.SubjectsTaught)
            .Include(yd => yd.Students)
            .Include(yd => yd.WeekPlanners)
            .Include(yd => yd.WeekPlannerTemplate)
            .ThenInclude(wp => wp.DayTemplates)
            .Include(yd => yd.WeekPlannerTemplate)
            .ThenInclude(wp => wp.DayTemplates)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<YearPlan?> GetById(YearPlanId yearPlanId, CancellationToken cancellationToken)
    {
        return await context.YearPlans
            .Where(yd => yd.Id == yearPlanId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<YearLevelValue>> GetYearLevelsTaught(Guid UserId, int calendarYear,
        CancellationToken cancellationToken)
    {
        var yearPlan = await context.YearPlans
            .Where(yd => yd.CalendarYear == calendarYear)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return yearPlan?.YearLevelsTaught.ToList() ?? [];
    }

    public async Task<WeekPlannerTemplateId?> GetWeekPlannerTemplateId(YearPlanId yearPlanId, CancellationToken cancellationToken)
    {
        var yearPlan = await context.YearPlans
            .Where(yd => yd.Id == yearPlanId)
            .Include(yd => yd.WeekPlannerTemplate)
            .FirstOrDefaultAsync(cancellationToken);

        if (yearPlan is null)
        {
            return null;
        }

        return yearPlan.WeekPlannerTemplate.Id;
    }

    /// <summary>
    /// Attempts to find the week planner for the given year, term number and week number.
    /// If not found, creates a new week planner and adds it to the context.
    /// Changes will still need to be saved by the caller via the unit of work.
    /// </summary>
    /// <param name="yearPlanId"></param>
    /// <param name="year"></param>
    /// <param name="termNumber"></param>
    /// <param name="weekNumber"></param>
    /// <param name="weekStart"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<WeekPlanner> GetOrCreateWeekPlanner(YearPlanId yearPlanId, int year, int termNumber, int weekNumber, DateOnly weekStart,
        CancellationToken cancellationToken)
    {
        var weekPlanner = await context.WeekPlanners
            .Where(wp => wp.YearPlanId == yearPlanId && wp.Year == year && wp.TermNumber == termNumber && wp.WeekNumber == weekNumber)
            .Include(wp => wp.DayPlans)
            .ThenInclude(dp => dp.LessonPlans)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (weekPlanner is null)
        {
            weekPlanner = new WeekPlanner(yearPlanId, year, termNumber, weekNumber, weekStart);
            context.Add(weekPlanner);
        }

        weekPlanner.SortDayPlans();
        return weekPlanner;
    }

    public void Update(YearPlan yearPlan)
    {
        if (!context.YearPlans.Local.Any(yp => yp.Id == yearPlan.Id))
        {
            context.YearPlans.Attach(yearPlan);
            context.Entry(yearPlan).State = EntityState.Modified;
        }
    }

    public Task<WeekPlanner?> GetWeekPlanner(YearPlanId yearPlanId, DateOnly weekStart, CancellationToken cancellationToken)
    {
        return context.WeekPlanners
            .Where(wp => wp.YearPlanId == yearPlanId && wp.WeekStart == weekStart)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}