using LessonFlow.Components.WeekPlanners;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;
using static LessonFlow.Shared.AppConstants;

namespace LessonFlow.Components.Pages;

public partial class WeekPlannerPage : ComponentBase
{
    [CascadingParameter] public AppState AppState { get; set; } = default!;
    internal int Year => AppState.CurrentYear;
    internal int TermNumber => AppState.CurrentTerm;
    internal int WeekNumber => AppState.CurrentWeek;

    [Inject] NavigationManager NavigationManager { get; set; } = default!;
    [Inject] ILogger<WeekPlannerPage> Logger { get; set; } = default!;
    [Inject] ITermDatesService TermDatesService { get; set; } = default!;
    [Inject] IUserRepository UserRepository { get; set; } = default!;
    [Inject] IWeekPlannerRepository WeekPlannerRepository { get; set; } = default!;
    [Inject] IYearDataRepository YearDataRepository { get; set; } = default!;
    [Inject] IUnitOfWork UnitOfWork { get; set; } = default!;

    private const string _gridTemplateCols = "minmax(0, 0.6fr) repeat(5, minmax(0, 1fr))";
    private string _gridRows = string.Empty;
    private bool _loading;
    private string? _error;
    private bool _editingBreaks;
    private bool _canNavigateToNextWeek;
    private bool _canNavigateToPreviousWeek;

    internal YearData YearData => AppState.CurrentYearData!;
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.CurrentYearData!.WeekPlannerTemplate;
    internal WeekPlanner WeekPlanner { get; set; } = null!;
    internal List<GridColumn> GridCols { get; set; } = [];
    internal User User { get; set; } = null!;
    internal DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    internal int SelectedYear { get; set; }
    internal int SelectedTerm { get; set; }
    internal int SelectedWeek { get; set; }
    internal bool EditingDuties => _editingBreaks;
    internal WeekPlanner? EditingWeekPlanner { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            if (AppState.User is null)
            {
                NavigationManager.NavigateTo("/Account/Login", replace: true);
                return;
            }
            else
            {
                User = AppState.User;
            }

            if (WeekNumber == 0 || TermNumber == 0 || Year == 0)
            {
                var inSchoolHolidays = TermDatesService.IsSchoolHoliday(DateTime.Now);
                AppState.CurrentWeek = inSchoolHolidays ? TermDatesService.GetNextWeekNumber(DateOnly.FromDateTime(DateTime.Now)) : TermDatesService.GetWeekNumber(DateTime.Now);
                AppState.CurrentTerm = TermDatesService.GetTermNumber(DateTime.Now);
                AppState.CurrentYear = DateTime.Now.Year;
            }

            SelectedYear = Year;
            SelectedTerm = TermNumber;
            SelectedWeek = WeekNumber;
            _canNavigateToNextWeek = CanNavigateToNextWeek();
            _canNavigateToPreviousWeek = CanNavigateToPreviousWeek();

            AppState.OnStateChanged += OnAppStateChanged;

            if (AppState.CurrentYearData?.WeekPlannerTemplate != null)
            {
                var weekPlanner = YearData.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == WeekNumber && wp.TermNumber == TermNumber);

                if (weekPlanner is null)
                {
                    try
                    {
                        weekPlanner = await WeekPlannerRepository.GetWeekPlanner(YearData.Id, Year, TermNumber, WeekNumber, default);
                        if (weekPlanner is null)
                        {
                            var weekStart = TermDatesService.GetFirstDayOfWeek(Year, TermNumber, WeekNumber);
                            weekPlanner = new WeekPlanner(YearData, Year, TermNumber, WeekNumber, weekStart);
                        }
                    }
                    catch (Exception)
                    {
                        var weekStart = TermDatesService.GetFirstDayOfWeek(Year, TermNumber, WeekNumber);
                        weekPlanner = new WeekPlanner(YearData, Year, TermNumber, WeekNumber, weekStart);
                    }
                }

                WeekPlanner = weekPlanner;
                InitialiseGrid();
            }
        }
        finally
        {
            _loading = false;
        }
    }

    private void OnAppStateChanged()
    {
        if (AppState.CurrentYearData?.WeekPlannerTemplate != null && GridCols.Count != 0)
        {
            InitialiseGrid();
            InvokeAsync(StateHasChanged);
        }
    }

    private void InitialiseGrid()
    {
        GridCols = [];
        var filledGridCells = new bool[5, WeekPlannerTemplate.Periods.Count];

        for (int i = 0; i < WeekPlanner.DayPlans.Count; i++)
        {
            var gridCol = new GridColumn(i + 2); // +2 because the css grid-col starts at 1 and we have the timeslot column
            GridCols.Add(gridCol);

            if (!WeekPlannerTemplate.DayTemplates[i].IsWorkingDay) continue;
            gridCol.IsWorkingDay = true;

            var dayPlan = WeekPlanner.DayPlans[i];
            if (dayPlan.LessonPlans.Count == 0) continue;

            var cells = new List<GridCell>();
            for (int j = 0; j < dayPlan.LessonPlans.Count; j++)
            {
                var lessonPlan = dayPlan.LessonPlans[j];
                var cell = new GridCell([], dayPlan.LessonPlans[j], gridCol, lessonPlan.StartPeriod);
                if (cell.Period.NumberOfPeriods == 1)
                {
                    cell.RowSpans.Add((cell.Period.StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET, cell.Period.StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET + 1));
                    cell.IsFirstCellInBlock = true;
                }
                else
                {
                    cell.RowSpans.Add((j, j + 2));
                    cell.SetRowSpans(1, cell.Period.NumberOfPeriods, WeekPlannerTemplate.Periods);
                }

                gridCol.Cells.Add(cell);

                for (int k = 0; k < lessonPlan.NumberOfPeriods; k++)
                {
                    if (lessonPlan.StartPeriod - 1 + k < WeekPlannerTemplate.Periods.Count)
                    {
                        filledGridCells[i, lessonPlan.StartPeriod - 1 + k] = true;
                    }
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            var dayTemplate = WeekPlannerTemplate.DayTemplates[i];

            if (!dayTemplate.IsWorkingDay) continue;

            for (int j = 0; j < WeekPlannerTemplate.Periods.Count; j++)
            {
                if (filledGridCells[i, j]) continue;

                var period = dayTemplate.Periods.FirstOrDefault(p => p.StartPeriod == j + 1);
                if (period is null)
                {
                    var templatePeriod = WeekPlannerTemplate.Periods.First(p => p.StartPeriod == j + 1);
                    period = templatePeriod.PeriodType switch
                    {
                        PeriodType.Lesson => new LessonPeriod("", templatePeriod.StartPeriod, 1),
                        PeriodType.Break => new BreakPeriod(templatePeriod.Name, templatePeriod.StartPeriod, 1),
                        _ => new LessonPeriod("", templatePeriod.StartPeriod, 1),
                    };
                }

                var cell = new GridCell([], period, GridCols[i], period.StartPeriod);
                cell.RowSpans.Add((cell.Period.StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET, cell.Period.StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET + 1));
                cell.IsFirstCellInBlock = true;
                if (cell.Period.NumberOfPeriods > 1)
                {
                    cell.SetRowSpans(1, cell.Period.NumberOfPeriods, WeekPlannerTemplate.Periods);
                }

                if (j > GridCols[i].Cells.Count)
                {
                    GridCols[i].Cells.Add(cell);
                }
                else
                {
                    GridCols[i].Cells.Insert(j, cell);
                }

                for (int k = 0; k < cell.Period.NumberOfPeriods; k++)
                {
                    filledGridCells[i, j + k] = true;
                }
            }
        }

        _gridRows = "50px 40px";
        foreach (var period in WeekPlannerTemplate.Periods)
        {
            if (period.PeriodType == PeriodType.Lesson || period.PeriodType == PeriodType.Nit)
            {
                //_gridRows += " minmax(0, 1.5fr)";
                _gridRows += " 60px";
            }
            else
            {
                //_gridRows += " minmax(0, 1fr)";
                _gridRows += " 40px";
            }
        }

        _gridRows += " 40px";
    }

    private async Task HandleGoToSelectedDateClicked()
    {
        _loading = true;
        _error = null;
        try
        {
            var selectedDate = TermDatesService.GetFirstDayOfWeek(SelectedYear, SelectedTerm, SelectedWeek);

            await LoadWeekPlannerForDate(selectedDate, SelectedYear, SelectedTerm, SelectedWeek);

            AppState.CurrentTerm = SelectedTerm;
            AppState.CurrentWeek = SelectedWeek;
            AppState.CurrentYear = SelectedYear;

            UpdateCanNavigate();
            StateHasChanged();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task HandleGoToNextWeek()
    {
        try
        {
            _loading = true;

            var nextWeek = TermDatesService.GetNextWeek(Year, TermNumber, WeekNumber);
            AppState.CurrentTerm = TermDatesService.GetTermNumber(nextWeek);
            AppState.CurrentWeek = TermDatesService.GetWeekNumber(nextWeek.Year, AppState.CurrentTerm, nextWeek);

            await LoadWeekPlannerForDate(nextWeek, AppState.CurrentYear, AppState.CurrentTerm, AppState.CurrentWeek);

            UpdateCanNavigate();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    private bool CanNavigateToNextWeek()
    {
        try
        {
            var _ = TermDatesService.GetNextWeek(Year, TermNumber, WeekNumber);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private async Task HandleGoToPreviousWeek()
    {
        try
        {
            _loading = true;

            var previousWeek = TermDatesService.GetPreviousWeek(Year, TermNumber, WeekNumber);
            AppState.CurrentTerm = TermDatesService.GetTermNumber(previousWeek);
            AppState.CurrentWeek = TermDatesService.GetWeekNumber(previousWeek);

            await LoadWeekPlannerForDate(previousWeek, AppState.CurrentYear, AppState.CurrentTerm, AppState.CurrentWeek);

            UpdateCanNavigate();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadWeekPlannerForDate(DateOnly nextWeek, int year, int term, int week)
    {
        if (!AppState.YearDataByYear.ContainsKey(nextWeek.Year))
        {
            var yearData = await YearDataRepository.GetByUserIdAndYear(User.Id, nextWeek.Year, new CancellationToken());
            if (yearData is null)
            {
                var schoolName = AppState.CurrentYearData.SchoolName;
                yearData = new YearData(User.Id, WeekPlannerTemplate, schoolName, nextWeek.Year);
            }

            AppState.AddNewYearData(nextWeek.Year, yearData);
        }

        AppState.CurrentYear = nextWeek.Year;

        var weekPlanner = AppState.CurrentYearData.WeekPlanners.FirstOrDefault(wp => wp.WeekStart == nextWeek);
        if (weekPlanner is null)
        {
            weekPlanner = await WeekPlannerRepository.GetWeekPlanner(AppState.CurrentYearData!.Id, nextWeek, new CancellationToken());
            if (weekPlanner is null)
            {
                weekPlanner = new WeekPlanner(AppState.CurrentYearData!, year, term, week, nextWeek);
            }

            YearData.WeekPlanners.Add(weekPlanner);
        }

        WeekPlanner = weekPlanner;
    }

    private bool CanNavigateToPreviousWeek()
    {
        try
        {
            var _ = TermDatesService.GetPreviousWeek(Year, TermNumber, WeekNumber);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private void SelectedYearChanged(ChangeEventArgs args)
    {
        var year = int.Parse((string)args.Value!);
        SelectedYear = year;
    }

    private void SelectedTermChanged(ChangeEventArgs args)
    {
        var term = int.Parse((string)args.Value!);
        SelectedTerm = term;
    }

    private void SelectedWeekChanged(ChangeEventArgs args)
    {
        var week = int.Parse((string)args.Value!);
        SelectedWeek = week;
    }

    private void UpdateCanNavigate()
    {
        _canNavigateToNextWeek = CanNavigateToNextWeek();
        _canNavigateToPreviousWeek = CanNavigateToPreviousWeek();
    }

    internal void NavigateToLessonPlanner(DateOnly date, GridCell cell)
    {
        NavigationManager.NavigateTo($"/LessonPlanner/{DateToUrlString(date)}/{cell.Period.StartPeriod}");
    }

    private static string DateToUrlString(DateOnly date) => $"{date.Year}-{date.Month}-{date.Day}";

    private void HandleToggleEditBreaks()
    {
        EditingWeekPlanner = new WeekPlanner(YearData, Year, TermNumber, WeekNumber, WeekPlanner.WeekStart);
        foreach (var dp in WeekPlanner.DayPlans)
        {
            EditingWeekPlanner.UpdateDayPlan(dp.Clone());
        }
        _editingBreaks = true;
    }

    private void HandleCancelChanges()
    {
        EditingWeekPlanner = null;
        _editingBreaks = false;
    }

    internal void HandleBreakNameChanged(DayOfWeek day, BreakPeriod breakPeriod, string newName)
    {
        if (EditingWeekPlanner is null) return;

        var dayPlan = EditingWeekPlanner.DayPlans.First(dp => dp.DayOfWeek == day);
        if (string.IsNullOrEmpty(newName))
        {
            if (dayPlan.BreakDutyOverrides.ContainsKey(breakPeriod.StartPeriod))
            {
                dayPlan.BreakDutyOverrides.Remove(breakPeriod.StartPeriod);
            }
            return;
        }

        if (!dayPlan.BreakDutyOverrides.TryAdd(breakPeriod.StartPeriod, newName))
        {
            dayPlan.BreakDutyOverrides[breakPeriod.StartPeriod] = newName;
        }
    }

    internal void HandleBeforeSchoolDutyChanged(DayOfWeek day, string newName)
    {
        if (EditingWeekPlanner is null) return;
        var dayPlan = EditingWeekPlanner.DayPlans.First(dp => dp.DayOfWeek == day);
        dayPlan.BeforeSchoolDuty = newName;
    }

    internal void HandleAfterSchoolDutyChanged(DayOfWeek day, string newName)
    {
        if (EditingWeekPlanner is null) return;
        var dayPlan = EditingWeekPlanner.DayPlans.First(dp => dp.DayOfWeek == day);
        dayPlan.AfterSchoolDuty = newName;
    }

    private async Task HandleSaveChanges()
    {
        if (EditingWeekPlanner is null) return;
        try
        {
            _error = null;
            _loading = true;

            foreach (var dayPlan in EditingWeekPlanner.DayPlans)
            {
                var originalDayPlan = WeekPlanner.DayPlans.First(dp => dp.DayOfWeek == dayPlan.DayOfWeek);
                originalDayPlan.BreakDutyOverrides = dayPlan.BreakDutyOverrides;
                originalDayPlan.BeforeSchoolDuty = dayPlan.BeforeSchoolDuty;
                originalDayPlan.AfterSchoolDuty = dayPlan.AfterSchoolDuty;
            }

            WeekPlannerRepository.Add(WeekPlanner);
            await UnitOfWork.SaveChangesAsync();

        }
        catch (Exception e)
        {
            // do something here when I work out what kind of errors might be thrown
        }
        finally
        {
            _loading = false;
            _editingBreaks = false;
            EditingWeekPlanner = null;
        }
    }

    public void Dispose()
    {
        if (AppState is not null)
        {
            AppState.OnStateChanged -= OnAppStateChanged;
        }
    }
}