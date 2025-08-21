using LessonFlow.Domain.Enums;

namespace LessonFlow.Api.Contracts.Curriculum;

public record CurriculumSubjectDto(
    Guid Id,
    string Name,
    IEnumerable<YearLevelDto> YearLevels);

public record YearLevelDto(
    YearLevelValue YearLevel,
    string LearningStandard,
    IEnumerable<CapabilityDto> Capabilities,
    IEnumerable<DispositionDto> Dispositions,
    IEnumerable<ConceptualOrganiserDto> ConceptualOrganisers);

public record CapabilityDto(
    string Name,
    IEnumerable<string> Descriptors);

public record DispositionDto(
    string Title,
    IEnumerable<string> DevelopedWhen);

public record ConceptualOrganiserDto(
    string Name,
    string WhatItIs,
    string WhyItMatters,
    IEnumerable<string> ConceptualUnderstandings,
    IEnumerable<ContentDescriptionDto> ContentDescriptions);

public record ContentDescriptionDto(
    Guid Id,
    string ContentDescription,
    string Topic,
    IEnumerable<string> CurriculumCodes);