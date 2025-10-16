using LessonFlow.Database;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class LessonPlanRepository(ApplicationDbContext context) : ILessonPlanRepository
{
    public void Add(LessonPlan lessonPlan)
    {
        context.Add(lessonPlan);
    }

    public async Task<List<LessonPlan>> GetByYearDataAndDate(YearDataId yearDataId, DateOnly date,
        CancellationToken cancellationToken)
    {
        var lessonPlans = await context.LessonPlans
            .Where(lp => lp.YearData.Id == yearDataId && lp.LessonDate == date)
            .Include(lp => lp.Resources)
            .ToListAsync(cancellationToken);

        return lessonPlans;
    }

    public async Task<LessonPlan?> GetByYearDataAndDateAndPeriod(YearDataId yearDataId, DateOnly date, int period,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lp => lp.YearData.Id == yearDataId)
            .Where(lp => lp.LessonDate == date)
            .Where(lp => lp.StartPeriod == period)
            .Include(lp => lp.Resources)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void UpdateLessonPlan(LessonPlan lessonPlan)
    {
        context.Update(lessonPlan);
    }

    public async Task<List<LessonPlan>?> GetLessonsByYearDataId(YearDataId yearDataId,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lessonPlan => lessonPlan.YearData.Id == yearDataId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LessonPlan>> GetByDate(YearDataId yearDataId, DateOnly date,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lp => lp.YearData.Id == yearDataId)
            .Where(lp => lp.LessonDate == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken cancellationToken)
    {
        return await context.Resources
            .Where(r => lessonPlan.Resources.ToList().Contains(r))
            .ToListAsync(cancellationToken);
    }

    public void DeleteLessonPlans(IEnumerable<LessonPlan> lessonPlans)
    {
        foreach (var lessonPlan in lessonPlans)
        {
            lessonPlan.ClearResources();
        }

        context.RemoveRange(lessonPlans);
    }
}