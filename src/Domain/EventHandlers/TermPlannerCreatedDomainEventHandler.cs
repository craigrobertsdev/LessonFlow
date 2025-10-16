using LessonFlow.Database;
using LessonFlow.Domain.TermPlanners.DomainEvents;
using LessonFlow.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class TermPlannerCreatedDomainEventHandler(ApplicationDbContext context, IUnitOfWork unitOfWork)
    : INotificationHandler<TermPlannerCreatedDomainEvent>
{
    public async Task Handle(TermPlannerCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var yearData = await context.YearData
            .Where(yd => yd.Id == notification.YearDataId)
            .FirstAsync(cancellationToken);

        yearData.AddTermPlanner(notification.TermPlanner);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}