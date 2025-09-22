using Bunit;
using LessonFlow.Components.AccountSetup;
using LessonFlow.Components.AccountSetup.State;

namespace LessonFlow.UnitTests.UI.AccountSetup;
public class ScheduleFormTests : TestContext
{
    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_6_6_1()
    {
        var weekPlanner = Helpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));


        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("6");

        component.Find("select#lesson-duration").Change("1");
        var col = component.Instance.GridCols[0];

        Assert.Single(col.Cells[0].RowSpans);
        Assert.True(col.Cells.Count == 8);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_2_2_6()
    {
        var weekPlanner = Helpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("2");

        component.Find("select#lesson-duration").Change("6");
        var col = component.Instance.GridCols[0];

        Assert.Equal(3, col.Cells[0].RowSpans.Count);
        Assert.Equal((2, 4), col.Cells[0].RowSpans[0]);
        Assert.Equal((5, 7), col.Cells[0].RowSpans[1]);
        Assert.Equal((8, 10), col.Cells[0].RowSpans[2]);
    }
}
