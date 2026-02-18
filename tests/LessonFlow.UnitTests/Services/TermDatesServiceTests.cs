using LessonFlow.Domain.ValueObjects;
using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.UnitTests.Services;

public class TermDatesServiceTests
{
    public static Dictionary<int, List<SchoolHoliday>> SchoolHolidaysByYear = new()
    {
        {
            2025,
            [
                new SchoolHoliday(1, new DateOnly(2025, 4, 12), new DateOnly(2025, 4, 27)),
                new SchoolHoliday(2, new DateOnly(2025, 7, 5), new DateOnly(2025, 7, 20)),
                new SchoolHoliday(3, new DateOnly(2025, 9, 27), new DateOnly(2025, 10, 12)),
                new SchoolHoliday(4, new DateOnly(2025, 12, 13), new DateOnly(2026, 1, 25))
            ]
        },
        {
            2026,
            [
                new SchoolHoliday(1, new DateOnly(2026, 4, 11), new DateOnly(2026, 4, 26)),
                new SchoolHoliday(2, new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 19)),
                new SchoolHoliday(3, new DateOnly(2026, 9, 26), new DateOnly(2026, 10, 11)),
                new SchoolHoliday(4, new DateOnly(2026, 12, 12), new DateOnly(2026, 12, 31))
            ]
        }
    };

    private readonly ITermDatesService _termDatesService;

    public TermDatesServiceTests()
    {
        _termDatesService = UnitTestHelpers.CreateTermDatesService();
    }

    [Fact]
    public void GetTermNumber_DateInTerm1_Returns1()
    {
        // Arrange
        var date = new DateOnly(2025, 2, 15); // Date in Term 1
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(1, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateInTerm2_Returns2()
    {
        var date = new DateOnly(2025, 5, 10); // Date in Term 2
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(2, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateInTerm3_Returns3()
    {
        var date = new DateOnly(2025, 7, 28); // Date in Term 3
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(3, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateInBetweenTerms2And3_Returns3()
    {
        var date = new DateOnly(2025, 7, 22); // Date between Term 2 and Term 3
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(3, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateBeforeTerm1_Returns1()
    {
        var date = new DateOnly(2025, 1, 1); // Date before Term 1
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(1, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateAfterTerm4_Returns4()
    {
        var date = new DateOnly(2025, 12, 31); // Date after Term 4
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(4, termNumber);
    }

    [Fact]
    public void GetWeekStart_ValidInput_ReturnsCorrectDate()
    {
        var year = 2025;
        var termNumber = 1;
        var weekNumber = 3; // Third week of Term 1
        var expectedDate = new DateOnly(2025, 2, 10); // Start date of the third week of Term 1

        var weekStart = _termDatesService.GetFirstDayOfWeek(year, termNumber, weekNumber);

        Assert.Equal(expectedDate, weekStart);
    }

    [Fact]
    public void GetWeekStart_WeekNumberExceedsTermDuration_ThrowsArgumentOutOfRangeException()
    {
        var year = 2025;
        var termNumber = 1;
        var weekNumber = 12;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _termDatesService.GetFirstDayOfWeek(year, termNumber, weekNumber));
    }

    [Theory]
    [MemberData(nameof(ValidTermDateGenerator))]
    public void GetWeekNumber_ValidDate_ReturnsCorrectWeekNumber(DateOnly date, int expected)
    {
        var weekNumber = _termDatesService.GetWeekNumber(date);
        Assert.Equal(expected, weekNumber);
    }

    [Fact]
    public void GetWeekNumber_DateOutsideTermDates_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2025, 12, 31);
        Assert.Throws<ArgumentOutOfRangeException>(() => _termDatesService.GetWeekNumber(date));
    }

    [Theory]
    [MemberData(nameof(NextWeekDataGenerator))]
    public void GetNextWeek_WhenCalled_ReturnsCorrectDate(int year, int term, int week, DateOnly expected)
    {
        var nextWeekStart = _termDatesService.GetNextWeek(year, term, week);

        Assert.Equal(expected, nextWeekStart);
    }

    [Theory]
    [MemberData(nameof(NextWeekDataExceptionGenerator))]
    public void GetNextWeek_WhenCalledWithBadData_ShouldThrowException(int year, int term, int week,
        ArgumentOutOfRangeException expected)
    {
        var exception = Record.Exception(() => _termDatesService.GetNextWeek(year, term, week));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(expected.Message, exception.Message);
    }

    [Theory]
    [MemberData(nameof(PreviousWeekDataGenerator))]
    public void GetPreviousWeek_WhenCalled_ReturnsCorrectDate(int year, int term, int week, DateOnly expected)
    {
        var previousWeekStart = _termDatesService.GetPreviousWeek(year, term, week);

        Assert.Equal(expected, previousWeekStart);
    }

    [Theory]
    [MemberData(nameof(PreviousWeekDataExceptionGenerator))]
    public void GetPreviousWeek_WhenCalledWithBadData_ShouldThrowException(int year, int term, int week,
        ArgumentOutOfRangeException expected)
    {
        var exception = Record.Exception(() => _termDatesService.GetPreviousWeek(year, term, week));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(expected.Message, exception.Message);
    }

    [Fact]
    public void InitialiseSchoolHolidays_WhenTermDatesByYearAvailable_CorrectlyInitialisesSchoolHolidays()
    {
        Assert.Equal(2, _termDatesService.SchoolHolidaysByYear.Count);

        for (int i = 0; i < _termDatesService.SchoolHolidaysByYear.Count; i++)
        {
            var schoolYear = _termDatesService.SchoolHolidaysByYear.Keys.ElementAt(i);
            var holidays = _termDatesService.SchoolHolidaysByYear[schoolYear];

            Assert.Equal(4, holidays.Count);
            foreach (var holiday in holidays)
            {
                var expected = SchoolHolidaysByYear[schoolYear].First(h => h.TermNumber == holiday.TermNumber);
                Assert.Equal(expected.StartDate, holiday.StartDate);
                Assert.Equal(expected.EndDate, holiday.EndDate);
            }
        }
    }

    [Theory]
    [MemberData(nameof(SchoolHolidayDateGenerator))]
    public void IsSchoolHoliday_DateInHoliday_ReturnsTrue(DateTime date)
    {
        var isHoliday = _termDatesService.IsSchoolHoliday(date);
        Assert.True(isHoliday);
    }

    [Theory]
    [MemberData(nameof(NonSchoolHolidayDateGenerator))]
    public void IsSchoolHoliday_DateNotInHoliday_ReturnsFalse(DateTime date)
    {
        var isHoliday = _termDatesService.IsSchoolHoliday(date);
        Assert.False(isHoliday);
    }

    [Theory]
    [MemberData(nameof(ValidNextTermDataGenerator))]
    public void GetWeekInNextTerm_NextTermExists_ReturnsNextTerm(int year, int term, int week, DateOnly termStart)
    {
        var nextTerm = _termDatesService.GetWeekInNextTerm(year, term, week);
        Assert.Equal(termStart, nextTerm);
    }

    [Theory]
    [MemberData(nameof(ValidPreviousTermDataGenerator))]
    public void GetWeekInPreviousTerm_PreviousTermExists_ReturnsPreviousTerm(int year, int term, int week,
        DateOnly termStart)
    {
        var previousTerm = _termDatesService.GetWeekInPreviousTerm(year, term, week);
        Assert.Equal(termStart, previousTerm);
    }

    public static TheoryData<DateOnly, int> ValidTermDateGenerator()
    {
        var data = new TheoryData<DateOnly, int>();
        data.Add(new DateOnly(2025, 3, 5), 6);
        data.Add(new DateOnly(2026, 1, 27), 1);

        return data;
    }

    public static TheoryData<int, int, int, DateOnly> NextWeekDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 1, 1, new DateOnly(2025, 2, 3));
        data.Add(2025, 1, 11, new DateOnly(2025, 4, 28));
        data.Add(2025, 2, 5, new DateOnly(2025, 6, 2));
        data.Add(2025, 4, 9, new DateOnly(2026, 1, 26));
        return data;
    }


    public static TheoryData<int, int, int, ArgumentOutOfRangeException> NextWeekDataExceptionGenerator()
    {
        var data = new TheoryData<int, int, int, ArgumentOutOfRangeException>();
        data.Add(2025, 0, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 5, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2026, 4, 9, new ArgumentOutOfRangeException("There is no next week available"));
        data.Add(2027, 1, 1, new ArgumentOutOfRangeException("There are no current term dates for that calendar year"));
        return data;
    }

    public static TheoryData<int, int, int, DateOnly> PreviousWeekDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 1, 2, new DateOnly(2025, 1, 27));
        data.Add(2025, 2, 1, new DateOnly(2025, 4, 7));
        data.Add(2025, 3, 5, new DateOnly(2025, 8, 11));
        data.Add(2026, 1, 1, new DateOnly(2025, 12, 8));
        data.Add(2025, 1, 5, new DateOnly(2025, 2, 17));
        return data;
    }


    public static TheoryData<int, int, int, ArgumentOutOfRangeException> PreviousWeekDataExceptionGenerator()
    {
        var data = new TheoryData<int, int, int, ArgumentOutOfRangeException>();
        data.Add(2025, 0, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 5, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 1, 1, new ArgumentOutOfRangeException("There is no previous week available"));
        return data;
    }

    public static TheoryData<DateTime> SchoolHolidayDateGenerator()
    {
        var data = new TheoryData<DateTime>();
        data.Add(new DateTime(2025, 4, 15));
        data.Add(new DateTime(2025, 7, 10));
        data.Add(new DateTime(2025, 10, 5));
        data.Add(new DateTime(2025, 12, 25));
        data.Add(new DateTime(2026, 4, 20));
        data.Add(new DateTime(2026, 7, 15));
        data.Add(new DateTime(2026, 10, 1));
        data.Add(new DateTime(2026, 12, 20));
        return data;
    }

    public static TheoryData<DateTime> NonSchoolHolidayDateGenerator()
    {
        var data = new TheoryData<DateTime>();
        data.Add(new DateTime(2025, 2, 15));
        data.Add(new DateTime(2025, 5, 10));
        data.Add(new DateTime(2025, 8, 20));
        data.Add(new DateTime(2025, 11, 5));
        data.Add(new DateTime(2026, 3, 15));
        data.Add(new DateTime(2026, 6, 10));
        data.Add(new DateTime(2026, 9, 20));
        data.Add(new DateTime(2026, 11, 25));
        return data;
    }

    public static TheoryData<int, int, int, DateOnly> ValidNextTermDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 1, 1, new DateOnly(2025, 4, 28));
        data.Add(2025, 3, 1, new DateOnly(2025, 10, 13));
        data.Add(2025, 4, 1, new DateOnly(2026, 1, 26));
        data.Add(2025, 1, 4, new DateOnly(2025, 5, 19));
        return data;
    }

    public static TheoryData<int, int, int, DateOnly> ValidPreviousTermDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 2, 1, new DateOnly(2025, 1, 27));
        data.Add(2025, 4, 1, new DateOnly(2025, 7, 21));
        data.Add(2026, 1, 1, new DateOnly(2025, 10, 13));
        data.Add(2025, 2, 4, new DateOnly(2025, 2, 17));
        return data;
    }
}