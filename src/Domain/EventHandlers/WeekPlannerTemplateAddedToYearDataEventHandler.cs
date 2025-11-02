using LessonFlow.Database;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Domain.YearPlans.DomainEvents;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Domain.EventHandlers;

public class WeekPlannerTemplateAddedToYearPlanEventHandler(
        IUnitOfWorkFactory factory,
    IAmbientDbContextAccessor<ApplicationDbContext> ambient,
    ITermDatesService termDatesService)
    : INotificationHandler<WeekPlannerTemplateAddedToYearPlanEvent>
{
    public async Task Handle(WeekPlannerTemplateAddedToYearPlanEvent notification, CancellationToken ct)
    {
        await using var uow = factory.Create();
        ApplicationDbContext context = ambient.Current!; 
        var yearPlan = await context.YearPlans
            .Where(yd => yd.WeekPlannerTemplate != null && yd.WeekPlannerTemplate.Id == notification.WeekPlannerTemplateId)
            .Include(yd => yd.WeekPlanners)
            .FirstAsync(ct);

        if (yearPlan.WeekPlanners.Count == 0)
        {
            yearPlan.AddWeekPlanner(new WeekPlanner(
                yearPlan.Id,
                yearPlan.CalendarYear,
                1,
                1,
                termDatesService.GetFirstDayOfWeek(yearPlan.CalendarYear, 1, 1)));
        }

        await uow.SaveChangesAsync(ct);
    }
}