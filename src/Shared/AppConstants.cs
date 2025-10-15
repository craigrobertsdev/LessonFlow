using System.Collections.Immutable;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Shared;

public static class AppConstants
{
    public const int WEEK_PLANNER_GRID_START_ROW_OFFSET = 2;

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
        { "Mathematics", "bg-red-200" },
        { "Language", "bg-yellow-200" },
        { "Science", "bg-green-200" },
        { "Humanities And Social Science", "bg-purple-200" },
        { "The Arts", "bg-pink-200" },
        { "Health and Physical Education", "bg-teal-200" },
        { "Technologies", "bg-orange-200" },
        { "Other", "bg-gray-200" }
    };


}