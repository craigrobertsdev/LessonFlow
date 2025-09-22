using LessonFlow.Components.AccountSetup;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;

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
        selectedCell.SetRowSpans(1, 3, weekPlannerTemplate.DayTemplates[0].Periods);

        var expected = new List<(int start, int end)>
        {
            (2, 4),
            (5, 6)
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

        selectedCell.SetRowSpans(1, 6, weekPlannerTemplate.DayTemplates[0].Periods);

        var expected = new List<(int start, int end)>
        {
            (2, 4),
            (5, 7),
            (8, 10)
        };

        Assert.Equal(expected, selectedCell.RowSpans);
    }
} 