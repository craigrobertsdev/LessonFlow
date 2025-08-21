using LessonFlow.Api.Database;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords.DomainEvents;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

public class WeekPlannerTemplateAddedToYearDataEventHandler(
    ApplicationDbContext context,
    IUnitOfWork unitOfWork,
    ITermDatesService termDatesService)
    : INotificationHandler<WeekPlannerTemplateAddedToYearDataEvent>
{
    public async Task Handle(WeekPlannerTemplateAddedToYearDataEvent notification, CancellationToken cancellationToken)
    {
        var yearData = await context.YearData
            .Where(yd => yd.WeekPlannerTemplate != null && yd.WeekPlannerTemplate.Id == notification.WeekPlannerTemplateId)
            .Include(yd => yd.WeekPlanners)
            .FirstAsync(cancellationToken);

        if (yearData.WeekPlanners.Count == 0)
        {
            yearData.AddWeekPlanner(new WeekPlanner(
                yearData,
                1,
                1,
                yearData.CalendarYear,
                termDatesService.GetWeekStart(yearData.CalendarYear, 1, 1)));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}