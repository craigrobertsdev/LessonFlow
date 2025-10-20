using LessonFlow.Domain.Common.Interfaces;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IRepository<T> where T : class, IAggregateRoot
{
}