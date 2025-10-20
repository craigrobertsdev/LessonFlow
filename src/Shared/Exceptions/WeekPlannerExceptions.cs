namespace LessonFlow.Exceptions;

public class TooManyDayPlansInWeekPlannerException()
    : BaseException("A week planner can only have 5 day plans", 400, "WeekPlanner.TooManyDayPlans");

public class WeekPlannerAlreadyExistsException()
    : BaseException("Week Planner already exists", 400, "WeekPlanner.AlreadyExists");

public class WeekPlannerNotFoundException() : BaseException("Week Planner not found", 404, "WeekPlanner.NotFound");

public class DayPlanNotFoundException(DateOnly date) : BaseException($"Day Plan not found for date:{date}", 404, "DayPlan.NotFound");

public class TermDatesNotFoundException(string message = "Term dates not found") : BaseException(message, 404, "TermDates.NotFound");