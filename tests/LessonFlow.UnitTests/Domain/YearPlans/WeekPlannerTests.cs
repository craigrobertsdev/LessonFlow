using LessonFlow.Domain.YearPlans;
using static LessonFlow.UnitTests.UnitTestHelpers;

namespace LessonFlow.UnitTests.Domain.YearPlans;
public class WeekPlannerTests
{
    [Fact]
    public void GetDayPlan_WhenDateInWeek_ReturnsDayPlan()
    {
        var weekPlanner = new WeekPlanner(null!, TestYear, FirstMonthOfSchool, FirstDayOfSchool, FirstDateOfSchool);
        var dayPlan = weekPlanner.GetDayPlan(FirstDateOfSchool.AddDays(2));

        Assert.NotNull(dayPlan);
        Assert.Equal(FirstDateOfSchool.AddDays(2), dayPlan.Date);
    }

    [Fact]
    public void GetDayPlan_WhenDateNotInWeek_ReturnsNull()
    {
        var weekPlanner = new WeekPlanner(null!, TestYear, FirstMonthOfSchool, FirstDayOfSchool, FirstDateOfSchool);
        var dayPlan = weekPlanner.GetDayPlan(FirstDateOfSchool.AddDays(7));

        Assert.Null(dayPlan);
    }
}
