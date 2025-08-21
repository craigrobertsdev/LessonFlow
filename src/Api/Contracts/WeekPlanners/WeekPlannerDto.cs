using LessonFlow.Api.Contracts.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Api.Contracts.WeekPlanners;

public record WeekPlannerDto(
    IEnumerable<DayPlanDto> DayPlans,
    WeekPlannerTemplateDto WeekPlannerTemplate,
    DateOnly WeekStart,
    int WeekNumber);

public record SchoolEventDto(
    Location Location,
    string Name,
    bool FullDay,
    DateTime EventStart,
    DateTime EventEnd)
{
    public static List<SchoolEventDto> CreateMany(IEnumerable<SchoolEvent> schoolEvents)
    {
        return schoolEvents.Select(se => new SchoolEventDto(
            se.Location,
            se.Name,
            se.FullDay,
            se.EventStart,
            se.EventEnd)).ToList();
    }
}