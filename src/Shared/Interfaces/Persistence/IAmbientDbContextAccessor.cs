using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IAmbientDbContextAccessor<TContext> where TContext : DbContext
{
    TContext? Current { get; set; }
}
