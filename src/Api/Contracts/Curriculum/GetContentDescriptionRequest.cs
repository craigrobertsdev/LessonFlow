namespace LessonFlow.Api.Contracts.Curriculum;

public record GetContentDescriptionRequest(Guid SubjectId, List<string> YearLevels);