namespace LessonFlow.Exceptions;

public class DuplicateCurriculumCodeException() : BaseException("Cannot add a duplicate curriculum code", 409,
    "TermPlan.Duplicate");