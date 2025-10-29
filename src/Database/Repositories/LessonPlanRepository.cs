using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class LessonPlanRepository(ApplicationDbContext context) : ILessonPlanRepository
{
    public void Add(LessonPlan lessonPlan)
    {
        var subject = context.Subjects.First(s => s.Name == lessonPlan.Subject.Name);
        lessonPlan.UpdateSubject(subject);
        context.Add(lessonPlan);
    }

    public async Task<List<LessonPlan>> GetByDayPlanAndDate(DayPlanId dayPlanId, DateOnly date,
        CancellationToken cancellationToken)
    {
        var lessonPlans = await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId && lp.LessonDate == date)
            .Include(lp => lp.Resources)
            .ToListAsync(cancellationToken);

        return lessonPlans;
    }

    public async Task<LessonPlan?> GetByDayPlanAndDateAndPeriod(DayPlanId dayPlanId, DateOnly date, int period,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == date)
            .Where(lp => lp.StartPeriod == period)
            .Include(lp => lp.Resources)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LessonPlan?> GetByDateAndPeriodStart(DayPlanId dayPlanId, DateOnly date, int period, CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == date)
            .Where(lp => lp.StartPeriod == period)
            .Include(lp => lp.Resources)
            .Include(lp => lp.Subject)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public bool UpdateLessonPlan(LessonPlan lessonPlan)
    {
        var existingLessonPlan = context.LessonPlans
            .Include(lp => lp.Resources)
            .Include(lp => lp.Subject)
            .FirstOrDefault(lp => lp.Id == lessonPlan.Id);

        if (existingLessonPlan is not null)
        {
            existingLessonPlan.UpdateValuesFrom(lessonPlan);
            var subject = context.Subjects.First(s => s.Id == lessonPlan.Subject.Id);
            existingLessonPlan.UpdateSubject(subject);

            return true;
        }

        return false;
    }

    public async Task<List<LessonPlan>?> GetLessonsByDayPlanId(DayPlanId dayPlanId,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lessonPlan => lessonPlan.DayPlanId == dayPlanId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LessonPlan>> GetByDate(DayPlanId dayPlanId, DateOnly date,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken cancellationToken)
    {
        return await context.Resources
            .Where(r => lessonPlan.Resources.ToList().Contains(r))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateResources(LessonPlan lessonPlan, CancellationToken cancellationToken)
    {
        var existingResources = await context.Resources
            .Where(r => lessonPlan.Resources.Contains(r))
            .ToListAsync(cancellationToken);

        throw new NotImplementedException();

        // lessonPlan.UpdateResources(existingResources);  <-- need to work out whether the current implementation correctly tracks changes
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