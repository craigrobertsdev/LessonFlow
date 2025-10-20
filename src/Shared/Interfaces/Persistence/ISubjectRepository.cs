using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ISubjectRepository
{
    Task<List<Subject>> GetCurriculumSubjects(CancellationToken cancellationToken);

    Task<List<Subject>> GetSubjectsById(List<SubjectId> subjects, CancellationToken cancellationToken);
}