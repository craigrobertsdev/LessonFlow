using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.Shared;

public class AppState
{
    public User User { get; set; } = null!;
    public YearData YearData { get; set; } = null!;
}
