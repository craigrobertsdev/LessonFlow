namespace LessonFlow.Components.WeekPlanners;

public class GridColumn
{
    public GridColumn(int col)
    {
        Col = col;
    }
    public int Col { get; set; }
    public List<GridCell> Cells { get; set; } = [];
    public bool IsWorkingDay { get; set; }
}
