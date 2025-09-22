using System.Collections.Immutable;
using LessonFlow.Domain.Enums;

namespace LessonFlow;

public static class AppConstants
{
    public static readonly ImmutableArray<DayOfWeek> WeekDays =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    public static readonly ImmutableArray<YearLevelValue> YearLevels =
    [
        YearLevelValue.Reception, YearLevelValue.Year1, YearLevelValue.Year2, YearLevelValue.Year3,
        YearLevelValue.Year4, YearLevelValue.Year5, YearLevelValue.Year6, YearLevelValue.Year7,
        YearLevelValue.Year8, YearLevelValue.Year9, YearLevelValue.Year10
    ];

    public static Dictionary<string, string> SubjectColours = new() 
    {
        { "English", "bg-blue-200" },
        {"Mathematics", "bg-red-200" },
        { "Language", "bg-yellow-200" }
    };


}