using LessonFlow.Api.Contracts.Curriculum;

namespace LessonFlow.Api.Contracts.LessonPlans;

public record LessonPlanResponse(
    LessonPlanDto? LessonPlan,
    List<CurriculumSubjectDto> Curriculum);