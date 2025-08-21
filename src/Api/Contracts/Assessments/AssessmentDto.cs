using LessonFlow.Domain.Enums;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Api.Contracts.Assessments;

public record AssessmentDto(
    Guid SubjectId,
    Guid StudentId,
    YearLevelValue YearLevel,
    AssessmentResultResponse AssessmentResult,
    string PlanningNotes,
    DateTime ConductedDateTime);

public record AssessmentResultResponse(
    string Comments,
    AssessmentGrade Grade,
    DateTime DateMarked);