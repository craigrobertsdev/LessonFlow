using LessonFlow.Database;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Shared.Extensions;

public static class ApplicationDbContextExtensions
{
    public static void AttachIfNotTracked<TEntity>(this ApplicationDbContext context, TEntity entity)
        where TEntity : class
    {
        var entry = context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            context.Attach(entity);
        }
    }
}
