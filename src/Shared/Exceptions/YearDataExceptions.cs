namespace LessonFlow.Shared.Exceptions;

public class TermPlannerAlreadyAssociatedException() : BaseException("Term planner already exists for this year.",
    400, "YearData.TermPlannerAlreadyExists");

public class YearDataNotFoundException() : BaseException("No YearData found", 404, "YearData.NotFound");