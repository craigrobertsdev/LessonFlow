using LessonFlow.Api.Contracts.Services;

namespace LessonFlow.Domain.ValueObjects;

public record SchoolTerm(int TermNumber, DateOnly StartDate, DateOnly EndDate)
{
    public int GetNumberOfWeeks()
    {
        var numberOfDays = (int)(EndDate.ToDateTime(new TimeOnly()) - StartDate.ToDateTime(new TimeOnly())).TotalDays;
        return (int)Math.Ceiling((decimal)numberOfDays / 7);
    }
}

public static class TermDateExtensions
{
    public static IEnumerable<SchoolTerm> ConvertFromDtos(this IEnumerable<TermDateDto> termDates)
    {
        return termDates.Select((td, i) =>
            new SchoolTerm(i + 1, GetDateFromString(td.StartDate), GetDateFromString(td.EndDate)));
    }

    private static DateOnly GetDateFromString(string date)
    {
        var dateParts = date.Split('-');
        return new DateOnly(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]));
    }
}