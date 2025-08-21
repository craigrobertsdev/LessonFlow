namespace LessonFlow.Api.Contracts.LessonPlans.CreateLessonPlan;

public record CreateLessonPlanRequest(
    Guid SubjectId,
    List<Guid> ContentDescriptionIds,
    string PlanningNotes,
    string PlanningNotesHtml,
    List<Guid>? ResourceIds,
    DateOnly LessonDate,
    int NumberOfPeriods,
    int StartPeriod);