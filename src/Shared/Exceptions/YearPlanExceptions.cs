namespace LessonFlow.Shared.Exceptions;

public class TermPlannerAlreadyAssociatedException() : BaseException("Term planner already exists for this year.",
    400, "YearPlans.TermPlannerAlreadyExists");

public class YearPlanNotFoundException() : BaseException("No YearPlans found", 404, "YearPlans.NotFound");