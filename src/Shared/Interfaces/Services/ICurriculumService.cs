using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Interfaces.Services;

public interface ICurriculumService
{
    List<Subject> CurriculumSubjects { get; }
    string GetSubjectName(SubjectId subjectId);
    List<string> GetSubjectNames();
    List<Subject> GetSubjectsByYearLevel(YearLevel yearLevel);
    List<Subject> GetSubjectsByNames(IEnumerable<string> names);
    Subject? GetSubjectByName(string name);

    List<Subject> GetSubjectsByYearLevels(IEnumerable<SubjectId> subjectIds,
        IEnumerable<YearLevel> yearLevelValues);

    Dictionary<YearLevel, List<ContentDescription>> GetContentDescriptions(SubjectId subjectId,
        List<YearLevel> yearLevels);
}