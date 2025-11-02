namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
