using Bunit;
using LessonFlow.Components.AccountSetup;
using LessonFlow.Components.AccountSetup.State;

namespace LessonFlow.UnitTests.UI.AccountSetup;
public class ScheduleFormTests : TestContext
{
    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_6_6_1()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
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
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_2_6()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("2");

        component.Find("select#lesson-duration").Change("6");
        var col = component.Instance.GridCols[0];

        Assert.Equal(3, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 8), col.Cells[0].RowSpans[1]);
        Assert.Equal((9, 11), col.Cells[0].RowSpans[2]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(3, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_4_6()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("4");

        component.Find("select#lesson-duration").Change("6");
        var col = component.Instance.GridCols[0];

        Assert.Equal(3, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 8), col.Cells[0].RowSpans[1]);
        Assert.Equal((9, 11), col.Cells[0].RowSpans[2]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(3, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_4_1()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("4");

        component.Find("select#lesson-duration").Change("1");
        var col = component.Instance.GridCols[0];

        Assert.Single(col.Cells[0].RowSpans);
        Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        for (int i = 0; i < day.Periods.Count; i++)
        {
            Assert.Equal(i + 1, day.Periods[i].StartPeriod);
        }
        Assert.Equal(8, day.Periods.Count);
    }
    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_3_1_3()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("3");
        component.Find("select#lesson-duration").Change("1");
        component.Find("select#lesson-duration").Change("3");

        var col = component.Instance.GridCols[0];

        Assert.Equal(2, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[0].RowSpans[1]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);

        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(5, day.Periods[2].StartPeriod);
        Assert.Equal(6, day.Periods[3].StartPeriod);
        Assert.Equal(7, day.Periods[4].StartPeriod);
        Assert.Equal(8, day.Periods[5].StartPeriod);

        Assert.Equal(6, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_3_4()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("3");
        component.Find("select#lesson-duration").Change("4");

        var col = component.Instance.GridCols[0];

        Assert.Equal(2, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 8), col.Cells[0].RowSpans[1]);

        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[2].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[3].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[4].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(5, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_6_3()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("6");
        component.Find("select#lesson-duration").Change("3");

        var col = component.Instance.GridCols[0];

        Assert.Equal(2, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[0].RowSpans[1]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(5, day.Periods[2].StartPeriod);
        Assert.Equal(6, day.Periods[3].StartPeriod);
        Assert.Equal(7, day.Periods[4].StartPeriod);
        Assert.Equal(8, day.Periods[5].StartPeriod);

        Assert.Equal(6, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_CorrectlyUpdatesColumnCells_1_6_4()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));

        component.InvokeAsync(() => component.Instance.SelectedCell = component.Instance.GridCols[0].Cells[0]);
        component.Find("select#lesson-duration").Change("6");
        component.Find("select#lesson-duration").Change("4");

        var col = component.Instance.GridCols[0];

        Assert.Equal(2, col.Cells[0].RowSpans.Count);
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((6, 8), col.Cells[0].RowSpans[1]);
        Assert.Equal((9, 10), col.Cells[3].RowSpans[0]); 
        Assert.Equal((10, 11), col.Cells[4].RowSpans[0]); 

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(7, day.Periods[3].StartPeriod);
        Assert.Equal(8, day.Periods[4].StartPeriod);

        Assert.Equal(5, day.Periods.Count);
    }

    [Fact]
    public void HandleLessonDurationChange_Lesson1_1To4_Lesson5_1To2_Lesson1_4_1()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("4");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[3]);
        component.Find("select#lesson-duration").Change("2");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("1");

        Assert.Equal(7, col.Cells.Count);

        var lesson1 = col.Cells[0];
        Assert.Equal((3, 4), lesson1.RowSpans[0]);
        Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[3].RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[4].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[5].RowSpans[0]);

        var lesson5 = col.Cells[6];
        Assert.Equal((9, 11), lesson5.RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(2, day.Periods[1].StartPeriod);
        Assert.Equal(3, day.Periods[2].StartPeriod);
        Assert.Equal(4, day.Periods[3].StartPeriod);
        Assert.Equal(5, day.Periods[4].StartPeriod);
        Assert.Equal(6, day.Periods[5].StartPeriod);
        Assert.Equal(7, day.Periods[6].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_1_5_2()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("5");
        component.Find("select#lesson-duration").Change("2");

        Assert.Equal(7, col.Cells.Count);

        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[2].RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[3].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[4].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[5].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[6].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(4, day.Periods[2].StartPeriod);
        Assert.Equal(5, day.Periods[3].StartPeriod);
        Assert.Equal(6, day.Periods[4].StartPeriod);
        Assert.Equal(7, day.Periods[5].StartPeriod);
        Assert.Equal(8, day.Periods[6].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_Lesson1_1To2_Lesson3_1To3()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("2");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[2]);
        component.Find("select#lesson-duration").Change("3");

        Assert.Equal(5, col.Cells.Count);

        var lesson1 = col.Cells[0];
        Assert.Equal((3, 5), lesson1.RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);

        var lesson3 = col.Cells[2];
        Assert.Equal((6, 8), lesson3.RowSpans[0]);
        Assert.Equal((9, 10), lesson3.RowSpans[1]);
        Assert.Equal((8, 9), col.Cells[3].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[4].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(4, day.Periods[2].StartPeriod);
        Assert.Equal(6, day.Periods[3].StartPeriod);
        Assert.Equal(8, day.Periods[4].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_1_5_6_5()
    {
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("5");
        component.Find("select#lesson-duration").Change("6");
        component.Find("select#lesson-duration").Change("5");

        Assert.Equal(4, col.Cells.Count);

        var lesson1 = col.Cells[0];
        Assert.Equal((3, 5), lesson1.RowSpans[0]);
        Assert.Equal((6, 8), lesson1.RowSpans[1]);
        Assert.Equal((9, 10), lesson1.RowSpans[2]);

        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[3].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[2].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(8, day.Periods[3].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_Lesson3_1To4To1()
    { 
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[3]);
        component.Find("select#lesson-duration").Change("4");
        component.Find("select#lesson-duration").Change("1");

        Assert.Equal(8, col.Cells.Count);

        var lesson3 = col.Cells[3];
        Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);
        Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
        Assert.Equal((6, 7), lesson3.RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[4].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[5].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[6].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[7].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(2, day.Periods[1].StartPeriod);
        Assert.Equal(3, day.Periods[2].StartPeriod);
        Assert.Equal(4, day.Periods[3].StartPeriod);
        Assert.Equal(5, day.Periods[4].StartPeriod);
        Assert.Equal(6, day.Periods[5].StartPeriod);
        Assert.Equal(7, day.Periods[6].StartPeriod);
        Assert.Equal(8, day.Periods[7].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_Lesson3_1To4_Lesson1_1To4()
    { 
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[3]);
        component.Find("select#lesson-duration").Change("4");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("4");

        Assert.Equal(5, col.Cells.Count);

        var lesson1 = col.Cells[0];
        Assert.Equal((3, 5), lesson1.RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Equal((6, 8), lesson1.RowSpans[1]);
        Assert.Equal((8, 9), col.Cells[2].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[3].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[4].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(7, day.Periods[3].StartPeriod);
        Assert.Equal(8, day.Periods[4].StartPeriod);
    }

    [Fact]
    public void HandleLessonDurationChange_Lesson3_1To2_Lesson5_1To2_Lesson1_1To5()
    { 
        var weekPlanner = UnitTestHelpers.GenerateWeekPlannerTemplate();
        var accountSetupState = new AccountSetupState(Guid.NewGuid()) { WeekPlannerTemplate = weekPlanner };
        var component = RenderComponent<ScheduleForm>(parameters => parameters.Add(p => p.State, accountSetupState));
        var col = component.Instance.GridCols[0];

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[3]);
        component.Find("select#lesson-duration").Change("2");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[5]);
        component.Find("select#lesson-duration").Change("2");

        component.InvokeAsync(() => component.Instance.SelectedCell = col.Cells[0]);
        component.Find("select#lesson-duration").Change("5");

        Assert.Equal(4, col.Cells.Count);

        var lesson1 = col.Cells[0];
        Assert.Equal((3, 5), lesson1.RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Equal((6, 8), lesson1.RowSpans[1]);
        Assert.Equal((8, 9), col.Cells[2].RowSpans[0]);
        Assert.Equal((9, 10), lesson1.RowSpans[2]);
        Assert.Equal((10, 11), col.Cells[3].RowSpans[0]);

        var day = weekPlanner.DayTemplates.First(d => d.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, day.Periods[0].StartPeriod);
        Assert.Equal(3, day.Periods[1].StartPeriod);
        Assert.Equal(6, day.Periods[2].StartPeriod);
        Assert.Equal(8, day.Periods[3].StartPeriod);
    }
}
