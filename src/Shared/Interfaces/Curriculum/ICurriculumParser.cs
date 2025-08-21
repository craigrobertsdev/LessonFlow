using LessonFlow.Domain.Curriculum;

namespace LessonFlow.Interfaces.Curriculum;

public interface ICurriculumParser
{
    Task<List<Subject>> ParseCurriculum();
}