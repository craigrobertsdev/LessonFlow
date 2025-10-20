namespace LessonFlow.Shared.Exceptions;

public class NoSubjectsFoundException() : BaseException("No subjects were found in the database", 404,
    "AustralianCurriculum.NotFound");

public class StrandHasSubstrandsException() : BaseException(
    "Cannot add content descriptions to a strand that has substrands", 404,
    "AustralianCurriculum.StrandHasSubstrands");

public class AttemptedToAddNonCurriculumSubjectException(string subjectName) : BaseException(
    $"Cannot add non-curriculum subjects when parsing the curriculum. Subject name: {subjectName}",
    400, "AustralianCurriculum.AddNonCurriculumSubject");