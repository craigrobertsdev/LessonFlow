using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Interfaces.Services;

public interface ICurriculumService
{
    List<Subject> CurriculumSubjects { get; }
    string GetSubjectName(SubjectId subjectId);
    List<string> GetSubjectNames();
    List<Subject> GetSubjectsByYearLevel(YearLevelValue yearLevel);
    List<Subject> GetSubjectsByName(IEnumerable<string> names);

    List<Subject> GetSubjectsByYearLevels(IEnumerable<SubjectId> subjectIds,
        IEnumerable<YearLevelValue> yearLevelValues);

    Dictionary<YearLevelValue, List<ContentDescription>> GetContentDescriptions(SubjectId subjectId,
        List<YearLevelValue> yearLevels);
}