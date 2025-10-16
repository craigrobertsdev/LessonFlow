namespace LessonFlow.Database;

public class DbContextSettings
{
    public const string SectionName = "ConnectionStrings";
    public string DefaultConnection { get; set; } = null!;
}