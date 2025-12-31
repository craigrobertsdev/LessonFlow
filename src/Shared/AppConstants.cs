using System.Collections.Immutable;
using LessonFlow.Domain.Enums;

namespace LessonFlow.Shared;

public static class AppConstants
{
    public const int WEEK_PLANNER_GRID_START_ROW_OFFSET = 2;
    public const int MAX_RESOURCE_UPLOAD_SIZE_IN_BYTES = 1024 * 1024 * 50;
    public const long MAX_USER_STORAGE_IN_BYTES = (long)1024 * 1024 * 1024 * 2; // 2GB
    public const int SOFT_DELETION_PERIOD_DAYS = 30;

    public static readonly ImmutableArray<DayOfWeek> WeekDays =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    public static readonly ImmutableArray<YearLevel> YearLevels =
    [
        YearLevel.Reception, YearLevel.Year1, YearLevel.Year2, YearLevel.Year3,
        YearLevel.Year4, YearLevel.Year5, YearLevel.Year6, YearLevel.Year7,
        YearLevel.Year8, YearLevel.Year9, YearLevel.Year10
    ];

    public static IReadOnlyDictionary<string, string> SubjectColours = new Dictionary<string, string>()
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