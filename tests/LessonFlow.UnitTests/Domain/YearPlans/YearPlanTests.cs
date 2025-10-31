using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.YearPlans;
using static LessonFlow.UnitTests.UnitTestHelpers;

namespace LessonFlow.UnitTests.Domain.YearPlans;
public class YearPlanTests
{
    [Fact]
    public void AddWeekPlanner_WhenCalled_AddsWeekPlannerToBothLookupDictionaries()
    {
        var userId = Guid.NewGuid();
        var yearPlan = new YearPlan(userId, new WeekPlannerTemplate(userId), "Test", TestYear);
        var weekPlanner = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
        yearPlan.AddWeekPlanner(weekPlanner);

        var weekPlannerFromDate = yearPlan.GetWeekPlanner(FirstDateOfSchool);
        var weekPlannerFromTermAndWeek = yearPlan.GetWeekPlanner(1, 1);
        Assert.NotNull(weekPlannerFromDate);
        Assert.NotNull(weekPlannerFromTermAndWeek);
        Assert.StrictEqual(weekPlannerFromDate, weekPlannerFromTermAndWeek);

        Assert.Equal(FirstDateOfSchool, weekPlannerFromDate.WeekStart);
        Assert.Equal(1, weekPlannerFromDate.TermNumber);
        Assert.Equal(1, weekPlannerFromDate.WeekNumber);
    }

    [Fact]
    public void GetWeekPlanner_WhenCalledWithTermAndWeek_ReturnsCorrectWeekPlanner()
    {
        var userId = Guid.NewGuid();
        var yearPlan = new YearPlan(userId, new WeekPlannerTemplate(userId), "Test", TestYear);
        var weekPlanner = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
        yearPlan.AddWeekPlanner(weekPlanner);

        var result = yearPlan.GetWeekPlanner(1, 1);
        Assert.NotNull(result);
        Assert.StrictEqual(weekPlanner, result);
    }

    [Fact]
    public void GetWeekPlanner_WhenCalledWithDate_ReturnsCorrectWeekPlanner()
    {
        var userId = Guid.NewGuid();
        var yearPlan = new YearPlan(userId, new WeekPlannerTemplate(userId), "Test", TestYear);
        var weekPlanner = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
        yearPlan.AddWeekPlanner(weekPlanner);

        var result = yearPlan.GetWeekPlanner(FirstDateOfSchool);
        Assert.NotNull(result);
        Assert.StrictEqual(weekPlanner, result);
    }
}
