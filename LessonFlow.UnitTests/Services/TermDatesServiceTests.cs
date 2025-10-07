using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Interfaces.Services;

namespace LessonFlow.UnitTests.Services;
public class TermDatesServiceTests
{
    private readonly ITermDatesService _termDatesService;

    public TermDatesServiceTests()
    {
        _termDatesService = Helpers.CreateTermDatesService();
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

        var weekStart = _termDatesService.GetWeekStart(year, termNumber, weekNumber);

        Assert.Equal(expectedDate, weekStart);
    }

    [Fact]
    public void GetWeekStart_WeekNumberExceedsTermDuration_ThrowsArgumentOutOfRangeException()
    {
        var year = 2025;
        var termNumber = 1;
        var weekNumber = 12; // Exceeds the number of weeks in Term 1

        Assert.Throws<ArgumentOutOfRangeException>(() => _termDatesService.GetWeekStart(year, termNumber, weekNumber));
    }

    [Fact]
    public void GetWeekNumber_ValidDate_ReturnsCorrectWeekNumber()
    {
        var date = new DateOnly(2025, 3, 5); // Date in the 6th week of Term 1
        var expectedWeekNumber = 6;
        var weekNumber = _termDatesService.GetWeekNumber(date);
        Assert.Equal(expectedWeekNumber, weekNumber);
    }

    [Fact]
    public void GetWeekNumber_DateOutsideTermDates_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2025, 12, 31); // Date outside any term dates
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
    public void GetNextWeek_WhenCalledWithBadData_ShouldThrowException(int year, int term, int week, ArgumentOutOfRangeException expected)
    {
        var exception = Record.Exception(() => _termDatesService.GetNextWeek(year, term, week));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Theory]
    [MemberData(nameof(PreviousWeekDataGenerator))]
    public void GetPreivousWeek_WhenCalled_ReturnsCorrectDate(int year, int term, int week, DateOnly expected)
    {
        var previousWeekStart = _termDatesService.GetPreviousWeek(year, term, week);

        Assert.Equal(expected, previousWeekStart);
    }

    [Theory]
    [MemberData(nameof(PreviousWeekDataExceptionGenerator))]
    public void GetPreviousWeek_WhenCalledWithBadData_ShouldThrowException(int year, int term, int week, ArgumentOutOfRangeException expected)
    {
        var exception = Record.Exception(() => _termDatesService.GetPreviousWeek(year, term, week));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }
        

    public static TheoryData<int, int, int, DateOnly> NextWeekDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 1, 1, new DateOnly(2025, 2, 3));
        data.Add(2025, 1, 11, new DateOnly(2025, 4, 28)); 
        data.Add(2025, 2, 5, new DateOnly(2025, 6, 2));
        data.Add(2025, 4, 9, new DateOnly(2026, 1, 27)); 
        return data;
    }

     
    public static TheoryData<int, int, int, ArgumentOutOfRangeException> NextWeekDataExceptionGenerator()
    {
        var data = new TheoryData<int, int, int, ArgumentOutOfRangeException>();
        data.Add(2025, 0, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 5, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2026, 4, 9, new ArgumentOutOfRangeException("There are no current term dates for that calendar year"));
        data.Add(2027, 1, 1, new ArgumentOutOfRangeException("There are no current term dates for that calendar year")); // First date of term 2
        return data;
    }

    public static TheoryData<int, int, int, DateOnly> PreviousWeekDataGenerator()
    {
        var data = new TheoryData<int, int, int, DateOnly>();
        data.Add(2025, 1, 2, new DateOnly(2025, 1, 27));
        data.Add(2025, 2, 1, new DateOnly(2025, 4, 7)); 
        data.Add(2025, 3, 5, new DateOnly(2025, 8, 11));
        data.Add(2026, 1, 1, new DateOnly(2025, 12, 8)); 
        return data;
    }

     
    public static TheoryData<int, int, int, ArgumentOutOfRangeException> PreviousWeekDataExceptionGenerator()
    {
        var data = new TheoryData<int, int, int, ArgumentOutOfRangeException>();
        data.Add(2025, 0, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 5, 1, new ArgumentOutOfRangeException("Requested term number doesn't exist"));
        data.Add(2025, 1, 1, new ArgumentOutOfRangeException("There are no current term dates for that calendar year"));
        return data;
    }
}
