using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Interfaces.Services;

public interface ITermDatesService
{
    IReadOnlyDictionary<int, List<SchoolTerm>> TermDatesByYear { get; }
    IReadOnlyDictionary<int, Dictionary<int, int>> TermWeekNumbers { get; }
    void SetTermDates(int year, List<SchoolTerm> termDates);
    DateOnly GetWeekStart(int year, int termNumber, int weekNumber);
    int GetTermNumber(DateOnly date);
    int GetTermNumber(DateTime date);
    int GetWeekNumber(int year, int termNumber, DateOnly weekStart);
    int GetWeekNumber(DateOnly date);
    int GetWeekNumber(DateTime date);
}