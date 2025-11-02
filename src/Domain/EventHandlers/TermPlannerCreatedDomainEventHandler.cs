using LessonFlow.Database;
using LessonFlow.Domain.TermPlanners.DomainEvents;
using LessonFlow.Shared.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class TermPlannerCreatedDomainEventHandler(IUnitOfWorkFactory factory, IAmbientDbContextAccessor<ApplicationDbContext> ambient)
    : INotificationHandler<TermPlannerCreatedDomainEvent>
{
    public async Task Handle(TermPlannerCreatedDomainEvent notification, CancellationToken ct)
    {
        await using var uow = factory.Create();
        ApplicationDbContext context = ambient.Current!;

        var yearPlan = await context.YearPlans
            .Where(yd => yd.Id == notification.YearPlanId)
            .FirstAsync(ct);

        yearPlan.AddTermPlanner(notification.TermPlanner);

        await uow.SaveChangesAsync(ct);
    }
}