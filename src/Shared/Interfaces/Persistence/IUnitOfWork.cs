using Microsoft.EntityFrameworkCore.Storage;

namespace LessonFlow.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    IDbContextTransaction BeginTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
}