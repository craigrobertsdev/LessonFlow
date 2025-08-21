namespace LessonFlow.Exceptions;

public class LessonPlansNotFoundException() : BaseException("No lesson plans were found", 500, "LessonPlanner.NotFound");

public class ConflictingLessonPlansException(int startPeriod)
    : BaseException($"An existing LessonPlan already covers period {startPeriod}", 500, "LessonPlanner.Conflict");