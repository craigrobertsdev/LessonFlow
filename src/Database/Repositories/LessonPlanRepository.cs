using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class LessonPlanRepository(IDbContextFactory<ApplicationDbContext> factory, IAmbientDbContextAccessor<ApplicationDbContext> ambient) : ILessonPlanRepository
{
    public void Add(LessonPlan lessonPlan)
    {
        var context = ambient.Current!;
        var subject = context.Subjects.First(s => s.Name == lessonPlan.Subject.Name);
        lessonPlan.UpdateSubject(subject);
        context.Add(lessonPlan);
    }

    public async Task<List<LessonPlan>> GetLessonPlan(DayPlanId dayPlanId, DateOnly date,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var lessonPlans = await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId && lp.LessonDate == date)
            .Include(lp => lp.Resources)
            .ToListAsync(ct);

        return lessonPlans;
    }

    public async Task<LessonPlan?> GetLessonPlan(DayPlanId dayPlanId, DateOnly date, int period, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var lessonplan = await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == date)
            .Where(lp => lp.StartPeriod == period)
            .Include(lp => lp.Resources)
            .Include(lp => lp.Subject)
            .FirstOrDefaultAsync(ct);

        return lessonplan;
    }

    public async Task<bool> UpdateLessonPlan(LessonPlan lessonPlan, CancellationToken ct)
    {
        var context = ambient.Current!;
        var existingLessonPlan = await context.LessonPlans
            .Where(lp => lp.Id == lessonPlan.Id)
            .Include(lp => lp.Resources)
            .Include(lp => lp.Subject)
            .FirstOrDefaultAsync(ct);

        if (existingLessonPlan is null) return false;

        existingLessonPlan.UpdateValuesFrom(lessonPlan);
        var subject = await context.Subjects.FirstAsync(s => s.Name == lessonPlan.Subject.Name, ct);
        existingLessonPlan.UpdateSubject(subject);

        return true;
    }

    public async Task<List<LessonPlan>?> GetLessonPlans(DayPlanId dayPlanId,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.LessonPlans
            .Where(lessonPlan => lessonPlan.DayPlanId == dayPlanId)
            .Include(lp => lp.Resources)
            .ToListAsync(ct);
    }

    public async Task<List<LessonPlan>> GetConflictingLessonPlans(DayPlanId dayPlanId, LessonPlan lessonPlan, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == lessonPlan.LessonDate)
            .Where(lp => lp.Id != lessonPlan.Id)
            .Where(lp => lp.StartPeriod > lessonPlan.StartPeriod && lp.StartPeriod <= lessonPlan.StartPeriod + lessonPlan.NumberOfPeriods)
            .ToListAsync(ct);
    }

    public async Task<List<LessonPlan>> GetByDate(DayPlanId dayPlanId, DateOnly date,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.LessonPlans
            .Where(lp => lp.DayPlanId == dayPlanId)
            .Where(lp => lp.LessonDate == date)
            .ToListAsync(ct);
    }

    public async Task<List<Resource>> GetResources(LessonPlan lessonPlan, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Resources
            .Where(r => lessonPlan.Resources.ToList().Contains(r))
            .ToListAsync(ct);
    }

    public async Task UpdateResources(LessonPlan lessonPlan, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var existingResources = await context.Resources
            .Where(r => lessonPlan.Resources.Contains(r))
            .ToListAsync(ct);

        throw new NotImplementedException();

        // lessonPlan.UpdateResources(existingResources);  <-- need to work out whether the current implementation correctly tracks changes
    }

    public void DeleteLessonPlans(IEnumerable<LessonPlan> lessonPlans)
    {
        using var context = factory.CreateDbContext();
        foreach (var lessonPlan in lessonPlans)
        {
            lessonPlan.ClearResources();
        }

        context.RemoveRange(lessonPlans);
    }
}