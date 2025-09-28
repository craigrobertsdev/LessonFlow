using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;
using Microsoft.AspNetCore.Components;

namespace LessonFlow.Components.AccountSetup;

public partial class ScheduleForm
{
    [CascadingParameter] public AccountSetupState State { get; set; } = default!;
    [Parameter] public Func<Task> SaveChanges { get; set; } = default!;
    WeekPlannerTemplate WeekPlannerTemplate => State.WeekPlannerTemplate;
    DayOfWeek[] weekDays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
    public List<GridColumn> GridCols = [];
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
        // for each day of the week, check if the 
        List<DayTemplate> templates = [];
        foreach (var day in weekDays)
        {
            var isWorkingDay = State.WorkingDays.Contains(day);
            if (!isWorkingDay)
            {
                templates.Add(new DayTemplate([], day, DayType.Nwd));
                continue;
            }

            var dayTemplate = WeekPlannerTemplate.DayTemplates.FirstOrDefault(dt => dt.DayOfWeek == day);
            if (dayTemplate is not null)
            {
                templates.Add(dayTemplate);
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

        GridCols = WeekPlannerTemplate.DayTemplates.Select((day, i) =>
        {
            var col = new GridColumn(i + 2); // +2 because the css grid-col starts at 1 and we have the timeslot column
            for (int j = 2; j < day.Periods.Count + 2; j++)
            {
                var cell = new GridCell([], day.Periods[j - 2], col);
                // -1 because the grid starts at 2 and the periods at 1
                if (cell.Period.NumberOfPeriods == 1 || cell.Period.PeriodType == PeriodType.Break)
                {
                    cell.RowSpans.Add((cell.Period.StartPeriod + 1, cell.Period.StartPeriod + 2));
                    cell.IsFirstCellInBlock = true;
                }
                else
                {
                    cell.RowSpans.Add((j, j + 1));
                    cell.SetRowSpans(1, cell.Period.NumberOfPeriods, WeekPlannerTemplate.Periods);
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

    private List<int> GetDurationOptions()
    {
        if (SelectedCell is null) return [];

        var start = WeekPlannerTemplate.Periods.FindIndex(p => p.StartPeriod == SelectedCell.Period.StartPeriod);
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
        SelectedCell.SetRowSpans(oldDuration, newDuration, WeekPlannerTemplate.Periods);

        ChangeLessonDuration(SelectedCell, oldDuration, newDuration);

        StateHasChanged();
    }

    private void ChangeLessonDuration(GridCell cell, int oldDuration, int newDuration)
    {
        var cells = cell.Column.Cells;
        var dayTemplate = WeekPlannerTemplate.DayTemplates[cell.Column.Col - 2];
        var templatePeriods = WeekPlannerTemplate.Periods;

        if (newDuration > oldDuration)
        {
            var idx = cells.FindIndex(c => c?.StartRow == cell.StartRow) + 1;
            var cellsToRemove = newDuration - oldDuration;
            List<GridCell> cellsPendingRemoval = [];

            // Look forward to see if there are any multi-period lessons that will be impacted by the increase and adjust cells accordingly first.
            var breaksCovered = 0;
            for (int i = 0; i < idx; i++)
            {
                if (cells[i].Period.PeriodType == PeriodType.Break)
                {
                    breaksCovered++;
                }
            }

            for (int i = idx; i < cells.Count; i++)
            {
                if (cells[i].Period.PeriodType == PeriodType.Break)
                {
                    breaksCovered++;
                    continue;
                }

                if (cells[i].Period.StartPeriod > cell.Period.StartPeriod + newDuration + breaksCovered - 1) break;

                if (cells[i].Period.NumberOfPeriods > 1)
                {
                    ChangeLessonDuration(cells[i], cells[i].Period.NumberOfPeriods, 1);
                }
            }

            while (cellsPendingRemoval.Count < cellsToRemove)
            {
                if (cells[idx].Period.PeriodType == PeriodType.Break)
                {
                    idx++;
                    continue;
                }
                cellsPendingRemoval.Add(cells[idx]);
                idx++;
            }

            foreach (var cellToRemove in cellsPendingRemoval)
            {
                cells.Remove(cellToRemove);
            }

            dayTemplate.RemovePeriods(cellsPendingRemoval.Select(c => c.Period));
        }
        else
        {
            var idx = templatePeriods.FindIndex(tp => tp.StartPeriod == cell.Period.StartPeriod);
            var templatePeriodsStartIndex = idx + newDuration;
            int breaksCovered = 0;
            var to = templatePeriodsStartIndex + 1 == cells.Count ? cells.Count : templatePeriodsStartIndex + 1;
            for (int i = idx; i < to; i++)
            {
                if (templatePeriods[i].PeriodType == PeriodType.Break)
                {
                    breaksCovered++;
                }
            }
            templatePeriodsStartIndex += breaksCovered;

            var templatePeriodsEndIndex = idx + oldDuration;
            breaksCovered = 0;
            to = templatePeriodsEndIndex + 1 >= templatePeriods.Count ? templatePeriods.Count : templatePeriodsEndIndex + 1;
            for (int i = idx; i < to; i++)
            {
                if (templatePeriods[i].PeriodType == PeriodType.Break)
                {
                    breaksCovered++;
                }
            }
            templatePeriodsEndIndex += breaksCovered;

            var nextLessonStartPeriod = templatePeriods[templatePeriodsStartIndex].StartPeriod;
            int cellsStartIdx = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Period.StartPeriod == nextLessonStartPeriod + 1)
                {
                    cellsStartIdx = i;
                    break;
                }
                else if (cells[i].Period.StartPeriod + 1 == nextLessonStartPeriod)
                {
                    cellsStartIdx = i + 1;
                    break;
                }
                else if (i == cells.Count - 1)
                {
                    cellsStartIdx = cells.Count - 1;
                }
            }

            while (templatePeriodsStartIndex < templatePeriodsEndIndex)
            {
                if (templatePeriods[templatePeriodsStartIndex].PeriodType == PeriodType.Break)
                {
                    templatePeriodsStartIndex++;
                    nextLessonStartPeriod++;
                    cellsStartIdx++;
                    continue;
                }
                var newPeriod = new LessonPeriod(string.Empty, templatePeriods[templatePeriodsStartIndex].StartPeriod, 1);
                var newCell = new GridCell([(nextLessonStartPeriod + 1, nextLessonStartPeriod + 2)], newPeriod, cell.Column);
                nextLessonStartPeriod++;
                if (cellsStartIdx >= cells.Count)
                {
                    cells.Add(newCell);
                }
                else
                {
                    cells.Insert(cellsStartIdx, newCell);
                }
                dayTemplate.AddPeriod(newPeriod);
                cellsStartIdx++;
                templatePeriodsStartIndex++;
            }
        }
    }

    private int GetEndRow(GridColumn col, GridCell cell, int row)
    {
        if (cell is null) return 1;
        if (cell.Period.NumberOfPeriods == 1 || row == col.Cells.Count - 1) return row + 2;

        return col.Cells[row]?.Period.PeriodType == PeriodType.Break
            ? row + 2
            : GetEndRow(col, cell, row + 1);
    }

    private async Task HandleBack()
    {
        State.ClearError();
        await SaveChanges();
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