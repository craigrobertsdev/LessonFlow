using LessonFlow.Api.Contracts.WeekPlanners;
using LessonFlow.Components.WeekPlanners;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Interfaces.Services;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;

namespace LessonFlow.Components.Pages;

public partial class WeekPlannerPage : ComponentBase
{
    [CascadingParameter] public AppState AppState { get; set; } = default!;
    [SupplyParameterFromQuery] private int WeekNumber { get; set; }
    [SupplyParameterFromQuery] private int TermNumber { get; set; }
    [SupplyParameterFromQuery] private int Year { get; set; }

    [Inject] NavigationManager NavigationManager { get; set; } = default!;
    [Inject] ILogger<WeekPlannerPage> Logger { get; set; } = default!;
    [Inject] ITermDatesService TermDatesService { get; set; } = default!;
    [Inject] IUserRepository UserRepository { get; set; } = default!;
    [Inject] IWeekPlannerRepository WeekPlannerRepository { get; set; } = default!;

    private const string _gridTemplateCols = "0.8fr repeat(5, 1fr)";
    private string _gridRows = string.Empty;
    private bool _loading;
    private string? _error;

    internal YearData YearData => AppState.YearData!;
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.YearData!.WeekPlannerTemplate;
    internal WeekPlanner WeekPlanner { get; set; } = null!;
    internal List<GridColumn> GridCols { get; set; } = [];
    internal User User { get; set; } = null!;

    internal int SelectedYear { get; set; }
    internal int SelectedTerm { get; set; }
    internal int SelectedWeek { get; set; }

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
        }

        if (WeekNumber == 0 || TermNumber == 0 || Year == 0)
        {
            WeekNumber = TermDatesService.GetWeekNumber(DateTime.Now);
            TermNumber = TermDatesService.GetTermNumber(DateTime.Now);
            Year = DateTime.Now.Year;
        }

        AppState.OnStateChanged += OnAppStateChanged;
        if (AppState.YearData?.WeekPlannerTemplate != null)
        {
            var weekPlanner = YearData.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == WeekNumber && wp.TermNumber == TermNumber);

            if (weekPlanner is null)
            {
                var weekStart = TermDatesService.GetWeekStart(Year, TermNumber, WeekNumber);
                weekPlanner = new WeekPlanner(YearData, Year, TermNumber, WeekNumber, weekStart);
            }

            WeekPlanner = weekPlanner;
            InitialiseGrid();
        }
    }

    private void OnAppStateChanged()
    {
        if (AppState.YearData?.WeekPlannerTemplate != null && GridCols.Count != 0)
        {
            InitialiseGrid();
            InvokeAsync(StateHasChanged);
        }
    }

    private void InitialiseGrid()
    {
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

    private void SelectedTermDateChanged(ChangeEventArgs e)
    {
        if (e.Value is null) return;
        var date = DateOnly.Parse((string)e.Value);
        SelectedYear = date.Year;
        SelectedTerm = TermDatesService.GetTermNumber(date);
        SelectedWeek = TermDatesService.GetWeekNumber(date);
    }

    private async Task HandleGoToSelectedDateClicked()
    {
        _loading = true;
        _error = null;
        try
        {
            if (Year != SelectedYear)
            {
                var yearData = await UserRepository.GetYearDataByYear(User.Id, SelectedYear, new CancellationToken());
                if (yearData is null)
                {
                    _error = $"Error trying to load data for calendar year {SelectedYear}";
                    _loading = false;
                    return;
                }

                AppState.YearData = yearData;
            }

            var weekPlanner = await WeekPlannerRepository.GetWeekPlanner(AppState.YearData!.Id, SelectedWeek, SelectedTerm, SelectedYear, new CancellationToken());
            if (weekPlanner is null)
            {
                var weekStart = TermDatesService.GetWeekStart(SelectedYear, SelectedTerm, SelectedWeek);
                weekPlanner = new WeekPlanner(AppState.YearData!, SelectedYear, SelectedTerm, SelectedWeek, weekStart);
            }

            AppState.YearData.WeekPlanners.Add(weekPlanner);
            WeekPlanner = weekPlanner;
            WeekNumber = weekPlanner.WeekNumber;
            TermNumber = weekPlanner.TermNumber;
            Year = weekPlanner.Year;
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
            var weekPlanner = await WeekPlannerRepository.GetWeekPlanner(AppState.YearData!.Id, nextWeek, new CancellationToken());
            SelectedTerm = TermDatesService.GetTermNumber(nextWeek);
            SelectedWeek = TermDatesService.GetWeekNumber(nextWeek);
            if (weekPlanner is null)
            {
                weekPlanner = new WeekPlanner(AppState.YearData!, SelectedYear, SelectedTerm, SelectedWeek, nextWeek);
            }

            WeekPlanner = weekPlanner;
            YearData.WeekPlanners.Add(weekPlanner);
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

    public void Dispose()
    {
        if (AppState is not null)
        {
            AppState.OnStateChanged -= OnAppStateChanged;
        }
    }
}