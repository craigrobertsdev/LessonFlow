using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database;

public sealed class AmbientDbContextAccessor<TContext> : IAmbientDbContextAccessor<TContext> 
    where TContext : DbContext
{
    private static readonly AsyncLocal<TContext?> _current = new();
    public TContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
