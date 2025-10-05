using LessonFlow.Api.Contracts.Curriculum;
using LessonFlow.Api.Contracts.Resources;

namespace LessonFlow.Api.Contracts.LessonPlans;

public record LessonPlanDto(
    Guid LessonPlanId,
    CurriculumSubjectDto Subject,
    string PlanningNotesHtml,
    List<ResourceDto> Resources,
    List<LessonCommentDto> Comments,
    int StartPeriod,
    int NumberOfPeriods);

public record LessonCommentDto(
    string Content,
    bool Completed,
    bool StruckOut,
    DateTime? CompletedDateTime);