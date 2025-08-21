using LessonFlow.Api.Database;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords.DomainEvents;
using LessonFlow.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

internal sealed class YearDataCreatedDomainEventHandler(ApplicationDbContext context, IUnitOfWork unitOfWork)
    : INotificationHandler<YearDataCreatedDomainEvent>
{
    public async Task Handle(YearDataCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(t => t.Id == notification.UserId)
            .FirstAsync(cancellationToken);

        user.AddYearData(notification.YearData);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}