using LessonFlow.Shared.Extensions;

namespace LessonFlow.UnitTests;
public class ExtensionsTests
{
    [Theory]
    [MemberData(nameof(DateOnlyGetWeekStartData))]
    public void DateOnly_GetWeekStart_ReturnsCorrectDate(DateOnly date, DateOnly expected)
    {
        // Act
        var weekStart = date.GetWeekStart();
        // Assert
        Assert.Equal(expected, weekStart);
    }

    public static TheoryData<DateOnly, DateOnly> DateOnlyGetWeekStartData =>
        new()
        {
            { new DateOnly(2024, 6, 17), new DateOnly(2024, 6, 17) }, // Monday
            { new DateOnly(2024, 6, 18), new DateOnly(2024, 6, 17) }, // Tuesday
            { new DateOnly(2024, 6, 19), new DateOnly(2024, 6, 17) }, // Wednesday
            { new DateOnly(2024, 6, 20), new DateOnly(2024, 6, 17) }, // Thursday
            { new DateOnly(2024, 6, 21), new DateOnly(2024, 6, 17) }, // Friday
            { new DateOnly(2024, 6, 22), new DateOnly(2024, 6, 17) }, // Saturday
            { new DateOnly(2024, 6, 23), new DateOnly(2024, 6, 17) }, // Sunday
        };
}
