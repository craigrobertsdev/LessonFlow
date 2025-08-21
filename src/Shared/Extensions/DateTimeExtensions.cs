namespace LessonFlow.Extensionss;

public static class DateTimeExtensions
{
    public static string GetCalendarDate(this DateOnly date)
    {
        return date.ToString("dd/MM/yyyy");
    }
}