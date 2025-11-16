using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
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
    public void AddWeekPlanner_WhenWeekPlannerStartDateAlreadyInList_ShouldUpdateExistingValuesAndRetainSameWeekPlanner()
    {
        var userId = Guid.NewGuid();
        var yearPlan = new YearPlan(userId, new WeekPlannerTemplate(userId), "Test", TestYear);
        var weekPlanner = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
        var lessonPlan1 = new LessonPlan(weekPlanner.DayPlans[0].Id, new Subject("", [], ""), PeriodType.Lesson, "", 1, 1, FirstDateOfSchool, []);
        weekPlanner.DayPlans[0].LessonPlans.Add(lessonPlan1);
        yearPlan.AddWeekPlanner(weekPlanner);

        var weekPlanner2 = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
        weekPlanner2.DayPlans[0].BeforeSchoolDuty = "Updated Duty";
        weekPlanner2.DayPlans[4].AfterSchoolDuty = "Updated Duty";
        var lessonPlan2 = new LessonPlan(weekPlanner2.DayPlans[0].Id, new Subject("", [], ""), PeriodType.Lesson, "", 1, 2, FirstDateOfSchool, []);
        weekPlanner2.DayPlans[0].LessonPlans.Add(lessonPlan2);
        yearPlan.AddWeekPlanner(weekPlanner2);

        var result = yearPlan.GetWeekPlanner(FirstDateOfSchool);
        Assert.NotNull(result);
        Assert.Equal(weekPlanner.Id, result.Id);
        Assert.Equal(weekPlanner2.DayPlans[0].BeforeSchoolDuty, result.DayPlans[0].BeforeSchoolDuty);
        Assert.Equal(weekPlanner2.DayPlans[4].AfterSchoolDuty, result.DayPlans[4].AfterSchoolDuty);
        Assert.Equal(2, result.DayPlans[0].LessonPlans.Count);
        Assert.Contains(result.DayPlans[0].LessonPlans, lp => lp.Id == lessonPlan1.Id);
        Assert.Contains(result.DayPlans[0].LessonPlans, lp => lp.Id == lessonPlan2.Id);
    }

    //[Fact]
    //public void AddWeekPlanner_When

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
