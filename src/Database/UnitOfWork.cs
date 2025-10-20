using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace LessonFlow.Database;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public IDbContextTransaction BeginTransaction()
    {
        return context.Database.BeginTransaction();
    }

    public async Task CommitTransaction(CancellationToken cancellationToken)
    {
        await context.Database.CommitTransactionAsync(cancellationToken);
    }
}