using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Shared.Interfaces.Services;

public interface ITermDatesService
{
    IReadOnlyDictionary<int, List<SchoolTerm>> TermDatesByYear { get; }
    IReadOnlyDictionary<int, Dictionary<int, int>> TermWeekNumbers { get; }
    IReadOnlyDictionary<int, List<SchoolHoliday>> SchoolHolidaysByYear { get; }
    List<int> AvailableYears { get; }
    void SetTermDates(int year, List<SchoolTerm> termDates);
    DateOnly GetFirstDayOfWeek(int year, int termNumber, int weekNumber);
    int GetTermNumber(DateOnly date);
    int GetTermNumber(DateTime date);
    int GetWeekNumber(int year, int termNumber, DateOnly weekStart);
    int GetWeekNumber(DateOnly date);
    int GetWeekNumber(DateTime date);
    int GetNextWeekNumber(DateOnly date);

    /// <summary>
    /// Gets the date of the next school week after the provided date. 
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if there are no future weeks stored in the app's database</exception>
    DateOnly GetNextWeek(DateOnly date);
    
    /// <summary>
    /// Gets the start date of the next school week.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="termNumber"></param>
    /// <param name="weekNumber"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if there are no term dates for the requested calendar year or if the requested term number is out of range (ie. term 5)</exception>
    DateOnly GetNextWeek(int year, int termNumber, int weekNumber);

    /// <summary>
    /// Gets the start date of the previous school week.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="termNumber"></param>
    /// <param name="weekNumber"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if there are no term dates for the requested calendar year or if the requested term number is out of range (ie. term 5)</exception>
    DateOnly GetPreviousWeek(int year, int termNumber, int weekNumber);

    /// <summary>
    /// Gets the start date of the same week number in the next term. If the week number exceeds the number of weeks in the next term, it returns the start date of the last week of that term.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="termNumber"></param>
    /// <param name="weekNumber"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if there are no term dates for the requested calendar year or if the requested term number is out of range (ie. term 5)</exception>"
    DateOnly GetWeekInNextTerm(int year, int termNumber, int weekNumber);
    
    /// <summary>
    /// Gets the start date of the same week number in the previous term. If the week number exceeds the number of weeks in the next term, it returns the start date of the last week of that term.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="termNumber"></param>
    /// <param name="weekNumber"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if there are no term dates for the requested calendar year or if the requested term number is out of range (ie. term 5)</exception>"
    DateOnly GetWeekInPreviousTerm(int year, int termNumber, int weekNumber);
    DateOnly GetLastWeekOfTerm(int year, int termNumber);
    bool IsSchoolHoliday(DateTime date);
}