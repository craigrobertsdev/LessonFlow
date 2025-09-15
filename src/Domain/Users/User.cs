using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.AspNetCore.Identity;

namespace LessonFlow.Domain.Users;

public class User : IdentityUser<Guid>
{
    private readonly List<YearData> _yearDataHistory = [];
    private readonly List<Resource> _resources = [];
    public bool AccountSetupComplete { get; set; }
    public AccountSetupState? AccountSetupState { get; set; }
    public int LastSelectedYear { get; set; }
    public DateOnly LastSelectedWeekStart { get; set; }
    public List<YearData> YearDataHistory => _yearDataHistory;
    public YearData? CurrentYearData => _yearDataHistory.FirstOrDefault(y => y.CalendarYear == LastSelectedYear);
    public List<Resource> Resources => _resources;

    public YearData? GetYearData(int year)
    {
        return _yearDataHistory.FirstOrDefault(yd => yd.CalendarYear == year);
    }

    public void AddYearData(YearData yearData)
    {
        if (!YearDataExists(yearData))
        {
            _yearDataHistory.Add(yearData);
        }
    }

    private bool YearDataExists(YearData yearData)
    {
        return YearDataExists(yearData.CalendarYear);
    }

    private bool YearDataExists(int year)
    {
        return _yearDataHistory.FirstOrDefault(yd => yd.CalendarYear == year) is not null;
    }

    public void CompleteAccountSetup(int lastSelectedYear, DateOnly lastSelectedWeekStart)
    {
        LastSelectedYear = lastSelectedYear;
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
        if (!_resources.Contains(resource))
        {
            _resources.Add(resource);
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