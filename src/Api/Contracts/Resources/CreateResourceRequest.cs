using LessonFlow.Domain.Enums;

namespace LessonFlow.Api.Contracts.Resources;

public record CreateResourceRequest(
    Stream FileStream,
    string Name,
    Guid SubjectId,
    bool IsAssessment,
    List<YearLevel> YearLevels,
    List<string> AssociatedStrands);