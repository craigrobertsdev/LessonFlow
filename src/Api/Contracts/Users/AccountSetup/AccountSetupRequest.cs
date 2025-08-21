using LessonFlow.Api.Contracts.PlannerTemplates;

namespace LessonFlow.Api.Contracts.Users.AccountSetup;

public record AccountSetupRequest(
    string FirstName,
    string SchoolName,
    int CalendarYear,
    List<string> SubjectsTaught,
    List<string> YearLevelsTaught,
    List<string> WorkingDays,
    WeekPlannerTemplateDto WeekPlannerTemplate);