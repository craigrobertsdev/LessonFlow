namespace LessonFlow.Shared.Extensions;

public static class DateTimeExtensions
{
    public static string GetCalendarDate(this DateOnly date)
    {
        return date.ToString("dd/MM/yyyy");
    }
}