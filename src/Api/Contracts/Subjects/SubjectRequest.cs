namespace LessonFlow.Api.Contracts.Subjects;

public record SubjectRequest(string Name, List<string> ContentDescriptionIds);