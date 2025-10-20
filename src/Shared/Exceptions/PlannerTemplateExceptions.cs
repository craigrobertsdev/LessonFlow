namespace LessonFlow.Shared.Exceptions;

public class WeekPlannerTemplateNotFoundException() : BaseException(
    "No WeekPlannerTemplate was found with the requested id",
    500,
    "PlannerTemplates.NotFound");

public class TemplatePeriodMismatchException(int sentPeriodCount, int requiredPeriodCount) : BaseException(
    $"{sentPeriodCount} periods were sent but {requiredPeriodCount} were required",
    500,
    "PlannerTemplates.BadRequest");