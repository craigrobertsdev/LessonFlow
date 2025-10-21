using LessonFlow.Domain.Curriculum;

namespace LessonFlow.Shared.Interfaces.Curriculum;

public interface ICurriculumParser
{
    Task<List<Subject>> ParseCurriculum(string subjectDirectory);
}