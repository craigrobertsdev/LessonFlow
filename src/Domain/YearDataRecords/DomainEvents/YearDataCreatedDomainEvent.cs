using LessonFlow.Domain.Common.Primatives;

namespace LessonFlow.Domain.YearDataRecords.DomainEvents;

public record YearDataCreatedDomainEvent(Guid Id, YearData YearData, int CalendarYear, Guid UserId)
    : DomainEvent(Id);