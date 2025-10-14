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

    private const string _gridTemplateCols = "0.8fr repeat(5, 1fr)";
    private string _gridRows = string.Empty;
    private bool _loading;
    private string? _error;
    private bool _canNavigateToNextWeek;
    private bool _canNavigateToPreviousWeek;
    private bool _canNavigateToNextTerm;
    private bool _canNavigateToPreviousTerm;

    internal YearData YearData => AppState.CurrentYearData!;
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.CurrentYearData!.WeekPlannerTemplate;
    internal WeekPlanner WeekPlanner { get; set; } = null!;
    internal List<GridColumn> GridCols { get; set; } = [];
    internal User User { get; set; } = null!;

    internal DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    internal int SelectedYear { get; set; }
    internal int SelectedTerm { get; set; }
    internal int SelectedWeek { get; set; }
    internal DateTime MaxCalendarDate { get; set; }
    internal DateTime MinCalendarDate { get; set; }

    protected override void OnInitialized()
    {
        if (AppState.User is null)
        {
            NavigationManager.NavigateTo("/Account/Login", replace: true);
            return;
        }
        else
        {
            User = AppState.User;
            MaxCalendarDate = TermDatesService.TermDatesByYear[TermDatesService.TermDatesByYear.Keys.Max()].Max(t => t.EndDate).ToDateTime(new TimeOnly(0, 0, 0));
            MinCalendarDate = TermDatesService.TermDatesByYear[TermDatesService.TermDatesByYear.Keys.Min()].Min(t => t.StartDate).ToDateTime(new TimeOnly(0, 0, 0));
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
                var weekStart = TermDatesService.GetFirstDayOfWeek(Year, TermNumber, WeekNumber);
                weekPlanner = new WeekPlanner(YearData, Year, TermNumber, WeekNumber, weekStart);
            }

            WeekPlanner = weekPlanner;
            InitialiseGrid();
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
                var cell = new GridCell([], dayPlan.LessonPlans[j], gridCol);
                if (cell.Period.NumberOfPeriods == 1)
                {
                    cell.RowSpans.Add((cell.Period.StartPeriod + 1, cell.Period.StartPeriod + 2));
                    cell.IsFirstCellInBlock = true;
                }
                else
                {
                    cell.RowSpans.Add((j, j + 1));
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

                var cell = new GridCell([], period, GridCols[i]);
                cell.RowSpans.Add((cell.Period.StartPeriod + 1, cell.Period.StartPeriod + 2));
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

    internal Dictionary<int, int> GetTermAndWeekNumbers()
    {
        return TermDatesService.TermWeekNumbers[Year];
    }

    internal void SelectedTermDateChanged(DateTime? date)
    {
        if (date is DateTime newDate)
        {
            SelectedYear = newDate.Year;
            SelectedTerm = TermDatesService.GetTermNumber(newDate);
            SelectedWeek = TermDatesService.GetWeekNumber(newDate);
            SelectedDate = DateOnly.FromDateTime(newDate);

        }
    }

    private async Task HandleGoToSelectedDateClicked()
    {
        _loading = true;
        _error = null;
        try
        {
            var selectedDate = TermDatesService.GetFirstDayOfWeek(SelectedYear, SelectedTerm, SelectedWeek);

            await LoadWeekPlannerForDate(selectedDate);

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

            SelectedTerm = TermDatesService.GetTermNumber(nextWeek);
            SelectedWeek = TermDatesService.GetWeekNumber(nextWeek.Year, SelectedTerm, nextWeek);

            await LoadWeekPlannerForDate(nextWeek);
            AppState.CurrentTerm = SelectedTerm;
            AppState.CurrentWeek = SelectedWeek;

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

            SelectedTerm = TermDatesService.GetTermNumber(previousWeek);
            SelectedWeek = TermDatesService.GetWeekNumber(previousWeek);

            await LoadWeekPlannerForDate(previousWeek);

            AppState.CurrentTerm = SelectedTerm;
            AppState.CurrentWeek = SelectedWeek;

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

    private async Task LoadWeekPlannerForDate(DateOnly nextWeek)
    {
        if (nextWeek.Year != SelectedYear)
        {
            SelectedYear = nextWeek.Year;
        }

        if (!AppState.YearDataByYear.ContainsKey(SelectedYear))
        {
            var yearData = await YearDataRepository.GetByUserIdAndYear(User.Id, SelectedYear, new CancellationToken());
            if (yearData is null)
            {
                var schoolName = AppState.CurrentYearData.SchoolName;
                yearData = new YearData(User.Id, WeekPlannerTemplate, schoolName, SelectedYear);
            }

            AppState.AddNewYearData(SelectedYear, yearData);
        }

        AppState.CurrentYear = SelectedYear;

        var weekPlanner = AppState.CurrentYearData.WeekPlanners.FirstOrDefault(wp => wp.WeekStart == nextWeek);
        if (weekPlanner is null)
        {
            weekPlanner = await WeekPlannerRepository.GetWeekPlanner(AppState.CurrentYearData!.Id, nextWeek, new CancellationToken());
            if (weekPlanner is null)
            {
                weekPlanner = new WeekPlanner(AppState.CurrentYearData!, SelectedYear, SelectedTerm, SelectedWeek, nextWeek);
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

    public void Dispose()
    {
        if (AppState is not null)
        {
            AppState.OnStateChanged -= OnAppStateChanged;
        }
    }
}