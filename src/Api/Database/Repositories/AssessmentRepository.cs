using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Api.Database.Repositories;

public class AssessmentRepository(ApplicationDbContext context) : IAssessmentRepository
{
    public async Task<List<Assessment>> GetAssessmentsById(List<AssessmentId> assessmentIds,
        CancellationToken cancellationToken)
    {
        return await context.Assessments
            .Where(x => assessmentIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}