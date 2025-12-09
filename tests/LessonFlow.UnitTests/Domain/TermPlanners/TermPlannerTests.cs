using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners;

namespace LessonFlow.UnitTests.Domain.TermPlanners;

public class TermPlannerTests
{
    [Fact]
    public void TermPlanner_WhenCreated_ShouldHaveListOfYearLevels()
    {
        var yearPlanId = new YearPlanId(Guid.NewGuid());
        var yearLevels = new List<YearLevel>
        {
            YearLevel.Reception, YearLevel.Year1
        };
        var calendarYear = 2025;

        var termPlanner = new TermPlanner(yearPlanId, calendarYear, yearLevels);
        Assert.Equal(yearLevels, termPlanner.YearLevels);
        Assert.Equal(calendarYear, termPlanner.CalendarYear);
        Assert.Equal(yearPlanId, termPlanner.YearPlanId);
        Assert.Empty(termPlanner.TermPlans);
    }

    [Fact]
    public void TermPlanner_WhenCreated_ShouldBuildListOfConceptualOrganisers()
    {
        //var yearPlanId = new YearPlanId(Guid.NewGuid());
        //var yearLevels = new List<YearLevel>
        //{
        //    YearLevel.Reception, YearLevel.Year1
        //};
        //var calendarYear = 2025;

        //var termPlanner = new TermPlanner(yearPlanId, calendarYear, yearLevels);
        //Assert.NotNull(termPlanner.PlannedConceptualOrganisers);
        //Assert.Empty(termPlanner.PlannedConceptualOrganisers);
        throw new NotImplementedException("Will be implemented when TermPlanner.PlannedConceptualOrganisers is implemented");
    }

    [Fact]
    public void TermPlan_WhenCreated_ShouldCreateTermPlansForEachTerm()
    {
        var yearPlanId = new YearPlanId(Guid.NewGuid());
        var yearLevels = new List<YearLevel>
        {
            YearLevel.Reception, YearLevel.Year1
        };
        var calendarYear = 2025;
        var termPlanner = new TermPlanner(yearPlanId, calendarYear, yearLevels);

        Assert.Equal(4, termPlanner.TermPlans.Count);
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(i + 1, termPlanner.TermPlans[i].TermNumber);
            Assert.Empty(termPlanner.TermPlans[i].Subjects);
        }
    }
}
