using LessonFlow.Api.Contracts.LessonPlans;

namespace LessonFlow.Api.Contracts.WeekPlanners;

public record DayPlanDto(DateOnly Date, List<LessonPlanDto> LessonPlans, List<SchoolEventDto> SchoolEvents, Dictionary<int, string>? BreakDutyOverrides = null);