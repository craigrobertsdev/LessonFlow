using LessonFlow.Domain.Enums;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.UnitTests.Domain.WeekPlannerTemplate;
public class WeekPlannerTemplateTests
{
    [Fact]
    public void AddPeriod_ShouldAddPeriodToAllDayTemplates()
    {
        // Arrange
        var weekPlannerTemplate = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var newPeriod = new TemplatePeriod(PeriodType.Lesson, 9, "Lesson 7", new TimeOnly(10, 0), new TimeOnly(11, 0));

        // Act
        weekPlannerTemplate.AddPeriod(newPeriod);

        // Assert
        foreach (var dayTemplate in weekPlannerTemplate.DayTemplates)
        {
            Assert.True(dayTemplate.Periods[^1].StartPeriod == newPeriod.StartPeriod);
        }

        Assert.True(weekPlannerTemplate.Periods[^1] == newPeriod);
    }

    [Fact]
    public void RemovePeriod_ShouldRemovePeriodFromAllDayTemplates()
    {
        // Arrange
        var weekPlannerTemplate = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var periodToRemove = weekPlannerTemplate.Periods[2]; 

        // Act
        weekPlannerTemplate.RemovePeriod(periodToRemove);
        
        // Assert
        foreach (var dayTemplate in weekPlannerTemplate.DayTemplates)
        {
            Assert.DoesNotContain(dayTemplate.Periods, p => p.StartPeriod == periodToRemove.StartPeriod);
        }
        Assert.DoesNotContain(weekPlannerTemplate.Periods, p => p == periodToRemove);
    }
}
