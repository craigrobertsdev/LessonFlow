using LessonFlow.Domain.Common.Primatives;

namespace LessonFlow.Domain.YearPlans.DomainEvents;

public record YearPlanCreatedDomainEvent(Guid Id, YearPlan YearPlan, int CalendarYear, Guid UserId)
    : DomainEvent(Id);