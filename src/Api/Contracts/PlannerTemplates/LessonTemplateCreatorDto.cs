using LessonFlow.Domain.Enums;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Api.Contracts.PlannerTemplates;

public record LessonTemplateCreatorDto(WeekPlannerTemplateDto WeekPlannerTemplate, List<string> Subjects);

public record WeekPlannerTemplateDto(List<TemplatePeriodDto> Periods, List<DayTemplateDto> DayTemplates);

public record DayTemplateDto(DayOfWeek DayOfWeek, DayType Type, List<LessonTemplateDto> Templates);

public record LessonTemplateDto(PeriodType PeriodType, int NumberOfPeriods, int StartPeriod, string? SubjectName, string? BreakDuty);

public record TemplatePeriodDto(string? Name, string StartTime, string EndTime, bool IsBreak);

public static class TemplateDtos
{
    public static List<TemplatePeriod> FromDtos(this IEnumerable<TemplatePeriodDto> dtos)
    {
        var periods = new List<TemplatePeriod>();
        foreach (var dto in dtos)
        {
            var period = new TemplatePeriod
            (
                dto.IsBreak ? PeriodType.Break : PeriodType.Lesson,
                dto.Name,
                TimeOnly.Parse(dto.StartTime),
                TimeOnly.Parse(dto.EndTime)
            );

            periods.Add(period);
        }

        return periods;
    }
}