using LessonFlow.Domain.Common.Interfaces;
using MediatR;

namespace LessonFlow.Domain.Common.Primatives;

public record DomainEvent(Guid Id) : INotification, IDomainEvent;