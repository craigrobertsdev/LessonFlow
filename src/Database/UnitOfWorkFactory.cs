using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database;

public sealed class UnitOfWorkFactory<TContext>(IDbContextFactory<TContext> factory, IAmbientDbContextAccessor<TContext> ambient)
    : IUnitOfWorkFactory where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _factory = factory;
    private readonly IAmbientDbContextAccessor<TContext> _ambient = ambient;

    public IUnitOfWork Create() => UnitOfWork<TContext>.Create(_factory, _ambient);

}
