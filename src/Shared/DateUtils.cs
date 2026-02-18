namespace LessonFlow.Shared;

public static class DateUtils
{
    public static bool LessThan(this DateTime dateTime, DateOnly dateOnly) =>
        dateTime.Year < dateOnly.Year
        || dateTime.Year == dateOnly.Year && dateTime.Month < dateOnly.Month
        || dateTime.Year == dateOnly.Year && dateTime.Month == dateOnly.Month && dateTime.Day < dateOnly.Day;


    public static bool GreaterThan(this DateTime dateTime, DateOnly dateOnly) =>
        dateTime.Year > dateOnly.Year
        || dateTime.Year == dateOnly.Year && dateTime.Month > dateOnly.Month
        || dateTime.Year == dateOnly.Year && dateTime.Month == dateOnly.Month && dateTime.Day > dateOnly.Day;

    public static bool GreaterThanOrEqualTo(this DateTime dateTime, DateOnly dateOnly) =>
        dateTime.GreaterThan(dateOnly) || dateTime.Equals(dateOnly);

    public static bool LessThan(this DateOnly dateOnly, DateTime dateTime) =>
        dateOnly.Year < dateTime.Year
        || dateOnly.Year == dateTime.Year && dateOnly.Month < dateTime.Month
        || dateOnly.Year == dateTime.Year && dateOnly.Month == dateTime.Month && dateOnly.Day < dateTime.Day;

    public static bool LessThanOrEqualTo(this DateTime dateTime, DateOnly dateOnly) =>
        dateTime.LessThan(dateOnly) || dateTime.Equals(dateOnly);

    public static bool GreaterThan(this DateOnly dateOnly, DateTime dateTime) =>
        dateOnly.Year > dateTime.Year
        || dateOnly.Year == dateTime.Year && dateOnly.Month > dateTime.Month
        || dateOnly.Year == dateTime.Year && dateOnly.Month == dateTime.Month && dateOnly.Day > dateTime.Day;

    public static bool GreaterThanOrEqual(this DateOnly dateOnly, DateTime dateTime) =>
        dateOnly.GreaterThan(dateTime) || dateOnly.Equals(dateTime);

    public static bool LessThanOrEqual(this DateOnly dateOnly, DateTime dateTime) =>
        dateOnly.LessThan(dateTime) || dateOnly.Equals(dateTime);
}