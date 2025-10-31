using LessonFlow.Database;
using LessonFlow.Domain.TermPlanners.DomainEvents;
using LessonFlow.Shared.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class TermPlannerCreatedDomainEventHandler(ApplicationDbContext context, IUnitOfWork unitOfWork)
    : INotificationHandler<TermPlannerCreatedDomainEvent>
{
    public async Task Handle(TermPlannerCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var yearPlan = await context.YearPlans
            .Where(yd => yd.Id == notification.YearPlanId)
            .FirstAsync(cancellationToken);

        yearPlan.AddTermPlanner(notification.TermPlanner);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}