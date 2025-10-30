namespace LessonFlow.Shared.Extensions;

public static class DateTimeExtensions
{
    public static string GetCalendarDate(this DateOnly date)
    {
        return date.ToString("dd/MM/yyyy");
    }

    public static DateOnly GetWeekStart(this DateOnly date)
    {
        if (date.DayOfWeek == DayOfWeek.Sunday)         {
            return date.AddDays(-6);
        }

        var diff = date.DayOfWeek - DayOfWeek.Monday;
        return date.AddDays(-((int)diff));
    }
}