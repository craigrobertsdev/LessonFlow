using LessonFlow.Database;
using LessonFlow.Domain.YearPlans.DomainEvents;
using LessonFlow.Shared.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class YearPlanCreatedDomainEventHandler(ApplicationDbContext context, IUnitOfWork unitOfWork)
    : INotificationHandler<YearPlanCreatedDomainEvent>
{
    public async Task Handle(YearPlanCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(t => t.Id == notification.UserId)
            .FirstAsync(cancellationToken);

        user.AddYearPlan(notification.YearPlan);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}