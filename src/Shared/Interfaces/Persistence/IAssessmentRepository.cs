using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Interfaces.Persistence;

public interface IAssessmentRepository : IRepository<Assessment>
{
    public Task<List<Assessment>> GetAssessmentsById(List<AssessmentId> assessmentIds,
        CancellationToken cancellationToken);
}