using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.AspNetCore.Identity;

namespace LessonFlow.Domain.Users;

public class User : IdentityUser<Guid>
{
    public bool AccountSetupComplete { get; set; }
    public AccountSetupState? AccountSetupState { get; set; }
    public int LastSelectedYear { get; set; }
    public DateOnly LastSelectedWeekStart { get; set; }
    public List<YearData> YearDataHistory { get; private set; } = [];
    public YearData? CurrentYearData => YearDataHistory.FirstOrDefault(y => y.CalendarYear == LastSelectedYear);
    public List<Resource> Resources { get; private set; } = [];

    public YearData? GetYearData(int year)
    {
        return YearDataHistory.FirstOrDefault(yd => yd.CalendarYear == year);
    }

    public void AddYearData(YearData yearData)
    {
        if (!YearDataExists(yearData))
        {
            YearDataHistory.Add(yearData);
        }
    }

    private bool YearDataExists(YearData yearData)
    {
        return YearDataExists(yearData.CalendarYear);
    }

    private bool YearDataExists(int year)
    {
        return YearDataHistory.FirstOrDefault(yd => yd.CalendarYear == year) is not null;
    }

    public void CompleteAccountSetup()
    {
        if (AccountSetupState is null) return;
        LastSelectedYear = AccountSetupState.CalendarYear;
        AccountSetupState = null;
        AccountSetupComplete = true;
    }

    public void SetLastSelectedYear(int year)
    {
        LastSelectedYear = year;
    }

    public void SetLastSelectedWeekStart(DateOnly weekStart)
    {
        LastSelectedWeekStart = weekStart;
    }

    public void AddResource(Resource resource)
    {
        if (!Resources.Contains(resource))
        {
            Resources.Add(resource);
        }
    }

    public void AddResources(IEnumerable<Resource> resources)
    {
        foreach (var resource in resources)
        {
            AddResource(resource);
        }
    }
}