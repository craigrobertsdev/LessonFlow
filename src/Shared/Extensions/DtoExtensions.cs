using LessonFlow.Api.Contracts.Resources;
using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Extensions;

namespace LessonFlow.Shared.Extensions;

public static class DtoExtensions
{
    public static List<DayPlanDto> ToDtos(this IEnumerable<DayPlan> dayPlans, IEnumerable<Resource> resources,
        IEnumerable<Subject> subjects)
    {
        return dayPlans.Select(dp => new DayPlanDto(
                dp.Date,
                dp.LessonPlans.ToDtos(resources, subjects),
                dp.SchoolEvents.ToDtos(),
                dp.BreakDutyOverrides?.ToDictionary()))
            .ToList();
    }


    public static List<SchoolEventDto> ToDtos(this IEnumerable<SchoolEvent> events)
    {
        return events.Select(e => new SchoolEventDto(
                e.Location,
                e.Name,
                e.FullDay,
                e.EventStart,
                e.EventEnd))
            .ToList();
    }

    public static List<ResourceDto> ToDtos(this IEnumerable<Resource> resources)
    {
        return resources.Select(r => new ResourceDto(
                r.Id,
                r.Name,
                r.Url,
                r.IsAssessment,
                r.YearLevels))
            .ToList();
    }
}