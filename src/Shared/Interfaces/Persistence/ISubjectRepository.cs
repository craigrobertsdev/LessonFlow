using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface ISubjectRepository
{
    Task AddCurriculum(List<Subject> subjects, CancellationToken ct);
    Task<List<Subject>> GetAllSubjects(CancellationToken ct);
    Task<List<Subject>> GetSubjectsByName(List<string> subjectNames, CancellationToken ct);
    Task<List<Subject>> GetSubjectsById(List<SubjectId> subjectIds, CancellationToken ct);
    Task<Subject?> GetSubjectById(SubjectId subjectId, CancellationToken ct);
    Task<List<Subject>> GetSubjectsByYearLevels(List<YearLevel> yearLevels,
        CancellationToken ct);
}