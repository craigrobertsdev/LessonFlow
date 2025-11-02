using LessonFlow.Database;
using LessonFlow.Domain.YearPlans.DomainEvents;
using LessonFlow.Shared.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class YearPlanCreatedDomainEventHandler(IUnitOfWorkFactory uowFactory, IAmbientDbContextAccessor<ApplicationDbContext> ambient)
    : INotificationHandler<YearPlanCreatedDomainEvent>
{
    public async Task Handle(YearPlanCreatedDomainEvent notification, CancellationToken ct)
    {
        await using var uow = uowFactory.Create();
        ApplicationDbContext context = ambient.Current!;

        var user = await context.Users
            .Where(t => t.Id == notification.UserId)
            .FirstAsync(ct);

        user.AddYearPlan(notification.YearPlan);

        await uow.SaveChangesAsync(ct);
    }
}