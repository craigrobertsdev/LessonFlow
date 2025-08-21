using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.YearDataRecords.DomainEvents;

public record WeekPlannerTemplateAddedToYearDataEvent(Guid Id, WeekPlannerTemplateId WeekPlannerTemplateId) : DomainEvent(Id);