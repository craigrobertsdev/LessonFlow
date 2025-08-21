namespace LessonFlow.Exceptions;

public class TermPlannerCreationException()
    : BaseException("TermPlanners must have either one year level or a list of year levels", 400,
        "TermPlanner.NoYearLevel");

public class TooManyYearLevelsException() : BaseException("A maximum of two year levels are allowed", 400,
    "TermPlanner.MoreThanTwoYearLevels");

public class DuplicateTermPlanException()
    : BaseException("Cannot add a duplicate term plan", 400, "TermPlanner.AlreadyExists");

public class TooManyTermPlansException() : BaseException("A maximum of four term plans are allowed", 400,
    "TermPlanner.MoreThanFour");

public class DuplicateTermNumberException() : BaseException("Cannot add a duplicate term number", 400,
    "TermPlanner.DuplicateTermNumber");

public class TermPlannerDoesNotBelongToTeacherException() : BaseException("Term planner does not belong to teacher",
    401,
    "TermPlanner.NotBelongToTeacher");

public class TermPlannerNotFoundException() : BaseException("Term planner not found", 404, "TermPlanner.NotFound");

public class TermPlanSubjectsAlreadySetException() : BaseException(
    "AustralianCurriculum have already been set for TermPlan", 404,
    "TermPlanner.SubjectsAlreadySet");