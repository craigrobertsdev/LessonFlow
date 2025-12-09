using Bunit;
using LessonFlow.Components.AccountSetup;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;

namespace LessonFlow.UnitTests.UI.AccountSetup;
public class TimingFormTests : BunitContext
{
    [Fact]
    public void ChangePeriodType_CorrectlyUpdatesWeekPlannerTemplate()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = Render<TimingForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        var select = component.FindAll("select[data-testid='period-type']")[0];
        select.Change("Break");

        var updated = component.Instance.State.WeekPlannerTemplate.Periods[0];
        Assert.Equal(PeriodType.Break, updated.PeriodType);

        foreach (var dayTemplate in component.Instance.State.WeekPlannerTemplate.DayTemplates)
        {
            var period = dayTemplate.Periods.First(p => p.StartPeriod == updated.StartPeriod);
            Assert.IsType<BreakTemplate>(period);
        }
    }
}
