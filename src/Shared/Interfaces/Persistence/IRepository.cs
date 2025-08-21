using LessonFlow.Domain.Common.Interfaces;

namespace LessonFlow.Interfaces.Persistence;

public interface IRepository<T> where T : class, IAggregateRoot
{
}