using LessonFlow.Domain.Enums;

namespace LessonFlow.Api.Contracts.TermPlanners.CreateTermPlanner;

public record CreateTermPlannerRequest(List<TermPlannerDto> TermPlans, List<YearLevelValue> YearLevels);