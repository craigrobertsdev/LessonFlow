namespace LessonFlow.Domain.Enums;

public enum YearLevelValue
{
    Reception = 0,
    Year1 = 1,
    Year2 = 2,
    Year3 = 3,
    Year4 = 4,
    Year5 = 5,
    Year6 = 6,
    Year7 = 7,
    Year8 = 8,
    Year9 = 9,
    Year10 = 10,
    Years1To2 = 15,
    Years3To4 = 16,
    Years5To6 = 17,
    Years7To8 = 18,
    Years9To10 = 19
}

public static class YearLevelValueExtensions
{
    public static string ToDisplayString(this YearLevelValue yearLevel)
    {
        return yearLevel switch
        {
            YearLevelValue.Reception => "Reception",
            YearLevelValue when (int)yearLevel > 0 && (int)yearLevel < 15 => $"Year {(int)yearLevel}",
            YearLevelValue when (int)yearLevel >= 15 && (int)yearLevel <= 17 =>
                $"Years {(int)yearLevel - 14} and {(int)yearLevel - 13}",
            _ => "No Value"
        };
    }

    /// <summary>
    ///     Returns a query string in the format "yearLevels=Year1&yearLevels=Year2"
    /// </summary>
    /// <param name="yearLevels"></param>
    /// <returns></returns>
    public static string ToQueryString(this List<YearLevelValue> yearLevels)
    {
        return string.Join("&", yearLevels.ConvertAll(yl => $"yearLevels={yl}"));
    }
}