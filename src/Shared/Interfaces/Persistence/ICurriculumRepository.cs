using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Interfaces.Persistence;

public interface ICurriculumRepository
{
    Task AddCurriculum(List<Subject> subjects, CancellationToken cancellationToken);
    Task<List<Subject>> GetAllSubjects(CancellationToken cancellationToken);
    Task<List<Subject>> GetSubjectsByName(List<string> subjectNames, CancellationToken cancellationToken);
    Task<List<Subject>> GetSubjectsById(List<SubjectId> subjectIds, CancellationToken cancellationToken);

    Task<List<Subject>> GetSubjectsByYearLevels(List<YearLevelValue> yearLevels,
        CancellationToken cancellationToken);
}