namespace LessonFlow.Api.Contracts.Authentication;

public record AccountDataResponse(TeacherResponse User, int CurrentTerm, int CurrentWeek);