namespace LessonFlow.Services.CurriculumParser.SACurriculum;

public class MathematicsParser() : BaseParser("Mathematics", _contentDescriptionEndings)
{
    private static readonly char[] _contentDescriptionEndings = ['*'];
}