using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.TermPlanners.DomainEvents;

public record TermPlannerCreatedDomainEvent(Guid Id, TermPlanner TermPlanner, YearDataId YearDataId)
    : DomainEvent(Id);