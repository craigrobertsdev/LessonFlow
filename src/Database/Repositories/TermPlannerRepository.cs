using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class TermPlannerRepository(ApplicationDbContext context) : ITermPlannerRepository
{
    public async Task<TermPlanner?> GetById(TermPlannerId id, CancellationToken cancellationToken)
    {
        return await context.TermPlanners.FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);
    }

    public async Task<TermPlanner?> GetByYearPlanIdAndYear(YearPlanId yearPlanId, int calendarYear,
        CancellationToken cancellationToken)
    {
        var termPlanner = await context.TermPlanners
            .AsNoTracking()
            .Where(yd => yd.YearPlanId == yearPlanId)
            .Where(yd => yd.CalendarYear == calendarYear)
            .Include(tp => tp.TermPlans)
            .FirstOrDefaultAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

        termPlanner.PopulateSubjectsForTerms(subjects);

        return termPlanner;
    }

    public void Add(TermPlanner termPlanner)
    {
        context.TermPlanners.Add(termPlanner);
    }

    public async Task Delete(TermPlannerId id, CancellationToken cancellationToken)
    {
        var termPlanner = await GetById(id, cancellationToken);

        if (termPlanner == null)
        {
            return;
        }

        context.TermPlanners.Remove(termPlanner);
    }
}