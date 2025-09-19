using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;
using Microsoft.AspNetCore.Components;

namespace LessonFlow.Components.AccountSetup;

public partial class ScheduleFormNew

{
    [CascadingParameter] public AccountSetupState State { get; set; } = default!;
    WeekPlannerTemplate WeekPlannerTemplate => State.WeekPlannerTemplate;
    DayOfWeek[] weekDays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
    List<GridColumn> GridCols = [];
    GridCell? _selectedCell;
    public GridCell? SelectedCell
    {
        get => _selectedCell;
        set
        {
            _selectedCell = value;
            StateHasChanged();
        }
    }

    string _gridRows = string.Empty;
    const string _gridTemplateCols = "0.8fr repeat(5, 1fr)";

    protected override void OnInitialized()
    {
        if (WeekPlannerTemplate.DayTemplates.Count == 0)
        {

            List<DayTemplate> templates = [];
            foreach (var day in weekDays)
            {
                var isWorkingDay = State.WorkingDays.Contains(day);
                if (!isWorkingDay)
                {
                    templates.Add(new DayTemplate([], day, DayType.Nwd));
                    continue;
                }

                int periodCount = 1;
                var periods = WeekPlannerTemplate.Periods.Select<TemplatePeriod, PeriodBase>(p =>
                {
                    return p.PeriodType switch
                    {
                        PeriodType.Lesson => new LessonPeriod(string.Empty, periodCount++, 1),
                        PeriodType.Break => new BreakPeriod(string.Empty, periodCount++, 1),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }).ToList();

                templates.Add(new DayTemplate(periods, day, DayType.WorkingDay));
            }

            State.WeekPlannerTemplate.SetDayTemplates(templates);
        }

        GridCols = WeekPlannerTemplate.DayTemplates.Select((day, i) =>
        {
            var col = new GridColumn(i + 2); // +2 because the css grid-col starts at 1 and we have the timeslot column
                                             // starting at 2 because the first column is for the timeslots
            for (int j = 2; j < day.Periods.Count + 2; j++)
            {
                var cell = new GridCell(j, day.Periods[j - 2]);
                cell.Column = col;
                if (cell.Period.NumberOfPeriods == 1 || cell.Row == cell.Period.StartPeriod - 1) // -1 because the grid starts at 2 and the periods at 1
                {
                    cell.IsFirstCellInBlock = true;
                }
                else
                {
                    cell.IsFirstCellInBlock = false;
                }

                col.Cells.Add(cell);
            }

            col.IsWorkingDay = day.IsWorkingDay;

            return col;
        })
        .ToList();

        _gridRows = "50px";
        foreach (var period in WeekPlannerTemplate.Periods)
        {
            if (period.PeriodType == PeriodType.Lesson || period.PeriodType == PeriodType.Nit)
            {
                _gridRows += " 1.5fr";
            }
            else
            {
                _gridRows += " 1fr";
            }
        }
    }

    private IEnumerable<int> GetDurationOptions()
    {
        if (SelectedCell is null) return [];

        var start = WeekPlannerTemplate.DayTemplates[SelectedCell.Column.Col - 2].Periods.FindIndex(p => p.StartPeriod == SelectedCell.Period.StartPeriod);
        var maxDuration = 0;
        for (int i = start; i < WeekPlannerTemplate.Periods.Count; i++)
        {
            if (WeekPlannerTemplate.Periods[i].PeriodType == PeriodType.Break)
            {
                continue;
            }

            maxDuration++;
        }

        return Enumerable.Range(1, maxDuration).ToList();
    }

    void HandleLessonDurationChange(ChangeEventArgs args)
    {
        if (SelectedCell is null) return;

        var newDuration = int.Parse((string)args.Value!);
        var oldDuration = SelectedCell.Period.NumberOfPeriods;
        if (newDuration == oldDuration) return;

        SelectedCell.Period.NumberOfPeriods = newDuration;

        var cells = SelectedCell.Column.Cells;
        var idx = SelectedCell.Column.Cells.FindIndex(c => c?.Row == SelectedCell.Row) + 1;

        if (newDuration > oldDuration)
        {
            var cellsRemoved = 0;
            while (cellsRemoved < newDuration - oldDuration)
            {
                if (cells[idx]?.Period.PeriodType == PeriodType.Break)
                {
                    idx++;
                    continue;
                }

                cells[idx] = null;
                cellsRemoved++;
                idx++;
            }
        }
        else
        {
            var cellsAdded = 0;
            var cellsToAdd = oldDuration - newDuration;
            while (cellsAdded < cellsToAdd)
            {
                if (cells[idx]?.Period.PeriodType == PeriodType.Break)
                {
                    idx++;
                    continue;
                }
                cells[idx] = new GridCell(idx + 2, new LessonPeriod(string.Empty, idx, 1))
                {
                    Column = SelectedCell.Column,
                    IsFirstCellInBlock = true
                };
                cellsAdded++;
                idx++;
            }
        }

        StateHasChanged();
    }

    private int GetEndRow(GridColumn col, GridCell cell, int row)
    {
        if (cell is null) return 1;
        if (cell.Period.NumberOfPeriods == 1 || row == col.Cells.Count - 1) return row + 2;

        return col.Cells[row]?.Period.PeriodType == PeriodType.Break
            ? row + 2
            : GetEndRow(col, cell, row + 1);
    }

    private void HandleBack()
    {
        State.ClearError();
        State.UpdateStep(AccountSetupStep.Timing, ChangeDirection.Back);
    }

    private async Task HandleSubmit()
    {
        try
        {
            State.SetLoading(true);
            State.ClearError();

            // Basic validation: ensure grid shape matches config (no forced subject assignment)
            for (int slotIndex = 0; slotIndex < WeekPlannerTemplate.Periods.Count; slotIndex++)
            {
                // foreach (var day in _dayTemplates.Where(d => d.IsWorkingDay))
                // {
                // 	if (day.Periods is null || day.Periods.Count <= slotIndex)
                // 	{
                // 		State.SetError("Schedule configuration mismatch. Please revisit the Timing step.");
                // 		return;
                // 	}
                // }
            }

            // Save current state
            // await UserRepository.UpdateAccountSetupState(User.Id, State);

            // Navigate to week planner (final step)
            // NavigationManager.NavigateTo("/WeekPlanner");
        }
        catch (Exception ex)
        {
            State.SetError(ex.Message);
            Console.Error.WriteLine($"Error during setup: {ex}");
        }
        finally
        {
            State.SetLoading(false);
        }
    }

}

public class GridColumn
{
    public GridColumn(int col)
    {
        Col = col;
    }
    public int Col { get; set; }
    public List<GridCell?> Cells { get; set; } = [];
    public bool IsWorkingDay { get; set; }
}

public record GridCell
{
    public GridCell(int row, PeriodBase period)
    {
        Period = period;
        Row = row;
    }
    public GridColumn Column { get; set; } = null!;
    public int Row { get; set; }
    public bool IsFirstCellInBlock { get; set; }
    public PeriodBase Period { get; set; }
}