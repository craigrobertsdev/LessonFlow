namespace LessonFlow.Exceptions;

public class StrandCreationException() : BaseException("Either substrands or contentDescriptions must be provided",
    400,
    "Subject.NeitherStrandNorSubstrand");

public class InvalidCurriculumSubjectIdException()
    : BaseException("One of the subject IDs was not a curriculum subject", 400, "Subject.NotFound");

public class SubjectNotFoundException(string subjectName) : BaseException(
    $"No subject found with the name of {subjectName}", 400,
    "Subject.NotFound");