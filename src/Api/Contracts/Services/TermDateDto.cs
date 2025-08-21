using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Api.Contracts.Services;

public record TermDateDto(
    string StartDate,
    string EndDate);

public static class TermDateDtoExtensions
{
    public static IEnumerable<TermDateDto> ToDtos(this IEnumerable<SchoolTerm> termDates)
    {
        return termDates.Select(td =>
            new TermDateDto(td.StartDate.ToString("yyyy-MM-dd"), td.EndDate.ToString("yyyy-MM-dd")));
    }
}