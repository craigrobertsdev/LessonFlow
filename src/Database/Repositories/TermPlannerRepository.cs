using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class TermPlannerRepository(IDbContextFactory<ApplicationDbContext> factory) : ITermPlannerRepository
{
    public async Task<TermPlanner?> GetById(TermPlannerId id, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.TermPlanners.FirstOrDefaultAsync(tp => tp.Id == id, ct);
    }

    public async Task<TermPlanner?> GetByYearPlanIdAndYear(YearPlanId yearPlanId, int calendarYear,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var termPlanner = await context.TermPlanners
            .AsNoTracking()
            .Where(yd => yd.YearPlanId == yearPlanId)
            .Where(yd => yd.CalendarYear == calendarYear)
            .Include(tp => tp.TermPlans)
            .FirstOrDefaultAsync(ct);

        if (termPlanner is null)
        {
            return null;
        }

        var subjectIds = termPlanner.TermPlans
            .Select(tp => tp.Subjects)
            .SelectMany(sl => sl.Select(s => s.Id))
            .ToList();

        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .AsNoTracking()
            .ToListAsync(ct);

        termPlanner.PopulateSubjectsForTerms(subjects);

        return termPlanner;
    }

    public void Add(TermPlanner termPlanner)
    {
        using var context = factory.CreateDbContext();
        context.TermPlanners.Add(termPlanner);
    }

    public async Task Delete(TermPlannerId id, CancellationToken ct)
    {
        var termPlanner = await GetById(id, ct);

        if (termPlanner == null)
        {
            return;
        }

        using var context = factory.CreateDbContext();
        context.TermPlanners.Remove(termPlanner);
    }
}