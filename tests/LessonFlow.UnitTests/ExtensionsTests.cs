using LessonFlow.Shared.Extensions;

namespace LessonFlow.UnitTests;
public class ExtensionsTests
{
    [Theory]
    [MemberData(nameof(DateOnlyGetWeekStartData))]
    public void DateOnly_GetWeekStart_ReturnsCorrectDate(DateOnly date, DateOnly expected)
    {
        var weekStart = date.GetWeekStart();
        Assert.Equal(expected, weekStart);
    }

    public static TheoryData<DateOnly, DateOnly> DateOnlyGetWeekStartData =>
        new()
        {
            { new DateOnly(2025, 1, 27), new DateOnly(2025, 1, 27) }, // Monday
            { new DateOnly(2025, 1, 28), new DateOnly(2025, 1, 27) }, // Tuesday
            { new DateOnly(2025, 1, 29), new DateOnly(2025, 1, 27) }, // Wednesday
            { new DateOnly(2025, 1, 30), new DateOnly(2025, 1, 27) }, // Thursday
            { new DateOnly(2025, 1, 31), new DateOnly(2025, 1, 27) }, // Friday
            { new DateOnly(2025, 2, 1), new DateOnly(2025, 1, 27) }, // Saturday
            { new DateOnly(2025, 2, 2), new DateOnly(2025, 1, 27) }, // Sunday
        };
}
