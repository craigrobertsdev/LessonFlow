namespace LessonFlow.UnitTests.UI.AccountSetup;
public class GridCellTests
{
    [Fact]
    public void GridCell_SetRowSpans_IncreasesRowsCorrectly_1_3()
    {
        var col = Helpers.GenerateGridColumn();
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        var selectedCell = col.Cells[0];
        selectedCell.Period.NumberOfPeriods = 3;
        selectedCell.SetRowSpans(1, 3, weekPlannerTemplate.Periods);

        var expected = new List<(int start, int end)>
        {
            (3, 5),
            (6, 7)
        };

        Assert.Equal(expected, selectedCell.RowSpans);
    }

    [Fact]
    public void GridCell_SetRowSpans_IncreasesRowsCorrectly_1_6()
    {
        var col = Helpers.GenerateGridColumn();
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        var selectedCell = col.Cells[0];
        selectedCell.Period.NumberOfPeriods = 6;

        selectedCell.SetRowSpans(1, 6, weekPlannerTemplate.Periods);

        var expected = new List<(int start, int end)>
        {
            (3, 5),
            (6, 8),
            (9, 11)
        };

        Assert.Equal(expected, selectedCell.RowSpans);
    }

    [Fact]
    public void GridCell_SetRowSpans_DecreasesRowsCorrectly_6_1()
    {
        var col = Helpers.GenerateGridColumn();
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        var selectedCell = col.Cells[0];
        selectedCell.Period.NumberOfPeriods = 6;
        selectedCell.SetRowSpans(1, 6, weekPlannerTemplate.Periods);
        selectedCell.Period.NumberOfPeriods = 1;
        selectedCell.SetRowSpans(6, 1, weekPlannerTemplate.Periods);
        var expected = new List<(int start, int end)>
        {
            (3, 4)
        };
        Assert.Equal(expected, selectedCell.RowSpans);
    } 
} 