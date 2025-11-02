using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class AssessmentRepository(IDbContextFactory<ApplicationDbContext> factory, IAmbientDbContextAccessor<ApplicationDbContext> ambient) : IAssessmentRepository
{
    public async Task<List<Assessment>> GetAssessmentsById(List<AssessmentId> assessmentIds,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Assessments
            .Where(x => assessmentIds.Contains(x.Id))
            .AsNoTracking()
            .ToListAsync(ct);
    }
}