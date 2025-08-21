using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Api.Contracts.Resources;

public record ResourceDto(
    ResourceId Id,
    string Name,
    string Url,
    bool IsAssessment,
    IEnumerable<YearLevelValue> YearLevels);