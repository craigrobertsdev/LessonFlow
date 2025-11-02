using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LessonFlow.Database;

public sealed class UnitOfWork<TContext>(IDbContextFactory<TContext> factory, IAmbientDbContextAccessor<TContext> ambient) : IUnitOfWork, IAsyncDisposable
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _factory = factory;
    private readonly IAmbientDbContextAccessor<TContext> _ambient = ambient;
    private TContext? _db;
    private IDbContextTransaction? _tx;

    private void EnsureDb()
    {
        if (_db is not null) return;
        _db = _factory.CreateDbContext();
        _ambient.Current = _db;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            if (_db is null) return;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public IDbContextTransaction BeginTransaction()
    {
        if (_db is null) throw new InvalidOperationException("Call IUnitOfWorkFactory.CreateAsync() first.");
        _tx ??= _db.Database.BeginTransaction();
        return _tx;
    }

    public async Task CommitTransaction(CancellationToken ct)
    {
        if (_tx is null) return;
        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_tx is not null)
            {
                await _tx.RollbackAsync();
                await _tx.DisposeAsync();
            }
        }
        finally
        {
            if (_db is not null)
            {
                await _db.DisposeAsync();
                _db = null;
            }
            _ambient.Current = null!;
        }
    }

    internal static UnitOfWork<TContext> Create(IDbContextFactory<TContext> factory,
        IAmbientDbContextAccessor<TContext> ambient)
    {
        var uow = new UnitOfWork<TContext>(factory, ambient);
        uow.EnsureDb();
        return uow;
    }
}