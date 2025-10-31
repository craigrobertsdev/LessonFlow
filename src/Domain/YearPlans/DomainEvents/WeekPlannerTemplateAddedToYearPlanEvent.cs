using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.YearPlans.DomainEvents;

public record WeekPlannerTemplateAddedToYearPlanEvent(Guid Id, WeekPlannerTemplateId WeekPlannerTemplateId) : DomainEvent(Id);