using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.YearPlans;
using Microsoft.AspNetCore.Identity;

namespace LessonFlow.Domain.Users;

public class User : IdentityUser<Guid>
{
    public bool AccountSetupComplete { get; set; }
    public AccountSetupState? AccountSetupState { get; set; }
    public int LastSelectedYear { get; set; }
    public DateOnly LastSelectedWeekStart { get; set; }
    public List<YearPlan> YearPlans { get; private set; } = [];
    public YearPlan? CurrentYearPlan => YearPlans.FirstOrDefault(y => y.CalendarYear == LastSelectedYear);
    public List<Resource> Resources { get; private set; } = [];

    public YearPlan? GetYearPlan(int year)
    {
        return YearPlans.FirstOrDefault(yd => yd.CalendarYear == year);
    }

    public void AddYearPlan(YearPlan yearPlan)
    {
        if (!YearPlanExists(yearPlan))
        {
            YearPlans.Add(yearPlan);
        }
    }

    private bool YearPlanExists(YearPlan yearPlan)
    {
        return YearPlanExists(yearPlan.CalendarYear);
    }

    private bool YearPlanExists(int year)
    {
        return YearPlans.FirstOrDefault(yd => yd.CalendarYear == year) is not null;
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