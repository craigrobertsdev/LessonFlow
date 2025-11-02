using Microsoft.EntityFrameworkCore.Storage;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChangesAsync(CancellationToken ct = default);
    IDbContextTransaction BeginTransaction();
    Task CommitTransaction(CancellationToken ct);
}