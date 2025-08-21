using LessonFlow.Api.Database;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Exceptions;
using LessonFlow.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Api.Services;

public class TermDatesService : ITermDatesService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<int, IEnumerable<SchoolTerm>> _termDatesByYear;

    // year, term number, number of weeks
    private readonly Dictionary<int, Dictionary<int, int>> _termWeekNumbers = [];


    public TermDatesService(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _termDatesByYear = Task.Run(LoadTermDates).Result;
        // To allow API to start without term dates on first run
        if (_termDatesByYear.Count == 0)
        {
            return;
        }

        _termWeekNumbers = InitialiseTermWeekNumbers();
    }

    public IReadOnlyDictionary<int, IEnumerable<SchoolTerm>> TermDatesByYear => _termDatesByYear.AsReadOnly();
    public IReadOnlyDictionary<int, Dictionary<int, int>> TermWeekNumbers => _termWeekNumbers;

    public void SetTermDates(int year, List<SchoolTerm> termDates)
    {
        _termDatesByYear[year] = termDates;
    }

    public DateOnly GetWeekStart(int year, int termNumber, int weekNumber)
    {
        if (termNumber is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(termNumber), "Term number must be between 1 and 4");
        }

        if (weekNumber < 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(weekNumber, nameof(weekNumber));
        }

        var term = _termDatesByYear[year].First(x => x.TermNumber == termNumber);

        var weekStart = term.StartDate.AddDays(7 * (weekNumber - 1));
        if (weekStart > term.EndDate)
        {
            throw new ArgumentException("Week number is greater than the number of weeks in the term");
        }

        return weekStart;
    }

    public int GetTermNumber(DateOnly date)
    {
        if (!_termDatesByYear.TryGetValue(date.Year, out var termDates))
        {
            throw new TermDatesNotFoundException();
        }

        return termDates.First(x => x.StartDate <= date && x.EndDate >= date).TermNumber;
    }

    public int GetTermNumber(DateTime date)
    {
        return GetTermNumber(DateOnly.FromDateTime(date));
    }

    public int GetWeekNumber(int year, int termNumber, DateOnly weekStart)
    {
        if (termNumber is < 1 or > 4)
        {
            throw new ArgumentException("Term number must be between 1 and 4");
        }

        var term = _termDatesByYear[year].First(x => x.TermNumber == termNumber);

        var weekNumber = (int)Math.Floor((double)(weekStart.DayNumber - term.StartDate.DayNumber) / 7) + 1;
        if (weekNumber > _termWeekNumbers[year][termNumber])
        {
            throw new ArgumentException("Week start is greater than the end of the term");
        }

        return weekNumber;
    }

    public int GetWeekNumber(DateOnly date)
    {
        var termNumber = GetTermNumber(date);
        var year = date.Year;

        var daysToSubtract = (int)date.DayOfWeek - 1;
        if (daysToSubtract < 0)
        {
            daysToSubtract += 7; // Handle Sunday (0) by wrapping to 7 days back
        }

        DateOnly weekStart = date.AddDays(-daysToSubtract);
        return GetWeekNumber(year, termNumber, weekStart);
    }
    
    public int GetWeekNumber(DateTime date)
    {
        return GetWeekNumber(DateOnly.FromDateTime(date));
    }

    private async Task<Dictionary<int, IEnumerable<SchoolTerm>>> LoadTermDates()
    {
        var termDates = await _dbContext.TermDates.ToListAsync();

        if (termDates.Count == 0)
        {
            return [];
        }

        var termDatesByYear = new Dictionary<int, IEnumerable<SchoolTerm>>();
        foreach (var termDate in termDates)
        {
            if (termDatesByYear.ContainsKey(termDate.StartDate.Year))
            {
                continue;
            }

            termDatesByYear.Add(termDate.StartDate.Year,
                termDates.Where(td => td.StartDate.Year == termDate.StartDate.Year));
        }

        return termDatesByYear;
    }

    private Dictionary<int, Dictionary<int, int>> InitialiseTermWeekNumbers()
    {
        var termWeekNumbers = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (year, termDates) in _termDatesByYear)
        {
            termWeekNumbers.Add(year, new Dictionary<int, int>());
            foreach (var termDate in termDates)
            {
                var weeks = (int)Math.Floor((double)(termDate.EndDate.DayNumber - termDate.StartDate.DayNumber) / 7) +
                            1;
                termWeekNumbers[year].Add(termDate.TermNumber, weeks);
            }
        }

        return termWeekNumbers;
    }
}