using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Api.Database.Repositories;

public class PlannerTemplateRepository(ApplicationDbContext context) : IPlannerTemplateRepository
{
    public async Task<WeekPlannerTemplate?> GetById(WeekPlannerTemplateId id, CancellationToken cancellationToken)
    {
        return await context.WeekPlannerTemplates
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WeekPlannerTemplate?> GetByTeacherId(Guid userId, CancellationToken cancellationToken)
    {
        var weekPlannerTemplate = await context.WeekPlannerTemplates
            .Where(dp => dp.UserId == userId)
            .Include(dp => dp.Periods)
            .Include(dp => dp.DayTemplates)
            .FirstOrDefaultAsync(cancellationToken);

        return weekPlannerTemplate;
    }

    public void Add(WeekPlannerTemplate weekPlannerTemplate)
    {
        context.Add(weekPlannerTemplate);
    }
}