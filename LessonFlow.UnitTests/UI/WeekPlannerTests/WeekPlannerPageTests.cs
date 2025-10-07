using Bunit;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace LessonFlow.UnitTests.UI.WeekPlannerTests;
public class WeekPlannerPageTests : TestContext
{
    private readonly AppState _appState;

    public WeekPlannerPageTests()
    {
        _appState = CreateAppState();
    }
    [Fact]
    public void InitialiseGrid_WhenNoLessonsPlanned_ShouldRenderFromWeekPlannerTemplate()
    {
        var component = RenderWeekPlannerPage(_appState);

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(8, col.Cells.Count);
            Assert.Equal((2, 3), col.Cells[0].RowSpans[0]);
            Assert.Equal((3, 4), col.Cells[1].RowSpans[0]);
            Assert.Equal((4, 5), col.Cells[2].RowSpans[0]);
            Assert.Equal((5, 6), col.Cells[3].RowSpans[0]);
            Assert.Equal((6, 7), col.Cells[4].RowSpans[0]);
            Assert.Equal((7, 8), col.Cells[5].RowSpans[0]);
            Assert.Equal((8, 9), col.Cells[6].RowSpans[0]);
            Assert.Equal((9, 10), col.Cells[7].RowSpans[0]);
        }
    }

    [Fact]
    public void InitialiseGrid_WhenAllLessonsPlanned_ShouldRenderFromDayPlan()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "1" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(8, col.Cells.Count);
            Assert.Equal((2, 3), col.Cells[0].RowSpans[0]);
            Assert.Equal((3, 4), col.Cells[1].RowSpans[0]);
            Assert.Equal((4, 5), col.Cells[2].RowSpans[0]);
            Assert.Equal((5, 6), col.Cells[3].RowSpans[0]);
            Assert.Equal((6, 7), col.Cells[4].RowSpans[0]);
            Assert.Equal((7, 8), col.Cells[5].RowSpans[0]);
            Assert.Equal((8, 9), col.Cells[6].RowSpans[0]);
            Assert.Equal((9, 10), col.Cells[7].RowSpans[0]);
        }

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(typeof(LessonPlan), col.Cells[0].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[1].Period.GetType());
            Assert.Equal(typeof(BreakPeriod), col.Cells[2].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[3].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[4].Period.GetType());
            Assert.Equal(typeof(BreakPeriod), col.Cells[5].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[6].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[7].Period.GetType());
        }
    }

    [Fact]
    public void InitialiseGrid_WhenSomeLessonsPlanned_ShouldRenderFromDayPlan()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].LessonPlans.RemoveAt(0);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "1" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);

        var col = component.Instance.GridCols[0];
        Assert.Equal(8, col.Cells.Count);
        Assert.Equal((2, 3), col.Cells[0].RowSpans[0]);
        Assert.Equal((3, 4), col.Cells[1].RowSpans[0]);
        Assert.Equal((4, 5), col.Cells[2].RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[3].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[4].RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[5].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[6].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[7].RowSpans[0]);

        Assert.Equal(typeof(LessonPeriod), col.Cells[0].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[1].Period.GetType());
        Assert.Equal(typeof(BreakPeriod), col.Cells[2].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[3].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[4].Period.GetType());
        Assert.Equal(typeof(BreakPeriod), col.Cells[5].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[6].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[7].Period.GetType());
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Friday)]
    public void InitialiseGrid_WhenNonWorkingDay_ShouldNotRenderCells(DayOfWeek nonWorkingDay)
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var idx = appState.CurrentYearData!.WeekPlannerTemplate.DayTemplates.FindIndex(d => d.DayOfWeek == nonWorkingDay);
        appState.CurrentYearData!.WeekPlannerTemplate.DayTemplates[idx] = new DayTemplate([], DayOfWeek.Monday, DayType.NonWorking);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "1" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);
        var col = component.Instance.GridCols[idx];
        Assert.Empty(col.Cells);
    }

    [Fact]
    public void SetRowSpans_WithMultiPeriodLessonAfterFirstBreak_ShouldSetCorrectly()
    {
        _appState.CurrentYearData!.WeekPlannerTemplate = CreateWeekPlannerTemplateWithMultiPeriodLessons();
        var component = RenderWeekPlannerPage(_appState);

        var col = component.Instance.GridCols[0];

        Assert.Equal(6, col.Cells.Count);
        Assert.Equal((2, 4), col.Cells[0].RowSpans[0]);

        Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
        Assert.Single(col.Cells[1].RowSpans);

        Assert.Equal((5, 7), col.Cells[2].RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[3].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[4].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[5].RowSpans[0]);
    }

    [Fact]
    public void GetTermWeeks_WhenCalled_ReturnsCorrectNumberOfWeeks()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "1" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(_appState);

        var termAndWeekNumbers = component.Instance.GetTermAndWeekNumbers();

        Assert.Equal(4, termAndWeekNumbers.Count);
        Assert.Equal(11, termAndWeekNumbers[1]);
        Assert.Equal(10, termAndWeekNumbers[2]);
        Assert.Equal(10, termAndWeekNumbers[3]);
        Assert.Equal(9, termAndWeekNumbers[4]);
    }

    [Fact]
    public void SelectCalendarDate_WhenInTermTime_SetsCorrectYearTermAndWeek()
    {
        var component = RenderWeekPlannerPage(_appState);

        component.Find("input#term-date").Change("2025-07-28");

        Assert.Equal(2025, component.Instance.SelectedYear);
        Assert.Equal(3, component.Instance.SelectedTerm);
        Assert.Equal(2, component.Instance.SelectedWeek);
    }

    [Theory]
    [InlineData("2025-01-01", 2025, 1, 1)]
    [InlineData("2025-04-14", 2025, 2, 1)]
    [InlineData("2025-07-05", 2025, 3, 1)]
    [InlineData("2025-09-27", 2025, 4, 1)]
    [InlineData("2025-12-13", 2026, 1, 1)]
    public void SelectCalendarDate_WhenInSchoolHolidays_SetsYearTermAndWeekToFirstWeekOfNextTerm(string date, int year, int termNumber, int weekNumber)
    {
        var component = RenderWeekPlannerPage(_appState);
        component.Find("input#term-date").Change(date);
        Assert.Equal(year, component.Instance.SelectedYear);
        Assert.Equal(termNumber, component.Instance.SelectedTerm);
        Assert.Equal(weekNumber, component.Instance.SelectedWeek);
    }

    [Fact]
    public async Task GoToSelectedDate_WhenDateInSchoolTermAndWeekPlannerExists_LoadsCorrectYearDataAndWeekPlanner()
    {
        var component = RenderWeekPlannerPage(_appState);

        component.Find("input#term-date").Change("2025-07-28");
        await component.Find("button#go-to-selected-date").ClickAsync(new MouseEventArgs());

        var instance = component.Instance;
        Assert.Equal(2025, instance.AppState.CurrentYearData!.CalendarYear);
        Assert.NotNull(instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 2 && wp.TermNumber == 3));
        Assert.Equal(3, instance.WeekPlanner.TermNumber);
        Assert.Equal(2, instance.WeekPlanner.WeekNumber);
    }

    [Fact]
    public void NavigateToNextWeek_WhenNextWeekWithinSameYear_LoadsNextWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "1" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);
        component.Find("button#next-week").Click();

        Assert.Equal(2, component.Instance.SelectedWeek);
        Assert.Equal(1, component.Instance.SelectedTerm);
        Assert.Equal(2, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(1, component.Instance.WeekPlanner.TermNumber);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 2 && wp.TermNumber == 1));
        Assert.Equal(2025, component.Instance.AppState.CurrentYearData!.CalendarYear);
    }

    [Fact]
    public void NavigateToNextWeek_WhenNextWeekIsNextYear_LoadsNextWeekPlannerWithNextYearData()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "9" },
            { "termNumber", "4" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);
        component.Find("button#next-week").Click();

        Assert.Equal(1, component.Instance.SelectedWeek);
        Assert.Equal(1, component.Instance.SelectedTerm);
        Assert.Equal(1, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(1, component.Instance.WeekPlanner.TermNumber);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 1 && wp.TermNumber == 1));
        Assert.Equal(2026, component.Instance.AppState.CurrentYearData!.CalendarYear);
    }

    [Fact]
    public void NavigateToNextWeek_WhenOutOfRange_ButtonCannotBeClicked()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void NavigateToPreviousWeek_WhenClicked_LoadsPreviousWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { "weekNumber", "2" },
            { "termNumber", "1" },
            { "year", "2025" }
        });
        navigationManager.NavigateTo(uri);
        var component = RenderWeekPlannerPage(appState);
        component.Find("button#previous-week").Click();
        Assert.Equal(1, component.Instance.SelectedWeek);
    }

    private IRenderedComponent<WeekPlannerPage> RenderWeekPlannerPage(AppState appState)
    {
        var component = base.RenderComponent<WeekPlannerPage>(p => p.Add(c => c.AppState, appState));
        return component;
    }

    private AppState CreateAppState()
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var userRepository = new Mock<IUserRepository>();
        var weekPlannerRepository = new Mock<IWeekPlannerRepository>();
        var logger = new Mock<ILogger<AppState>>();
        var termDatesService = Helpers.CreateTermDatesService();

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        var yearData = new YearData(Guid.NewGuid(), new AccountSetupState(Guid.NewGuid()));
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearDataByYear.Add(yearData.CalendarYear, yearData);
        appState.User = new User();

        var weekPlanner = new WeekPlanner(yearData, 2025, 3, 2, new DateOnly(2025, 7, 28));
        weekPlannerRepository.Setup(wp => wp.GetWeekPlanner(yearData.Id, 2025, 3, 2, new CancellationToken()).Result).Returns(weekPlanner);

        Services.AddScoped(sp => termDatesService);
        Services.AddScoped(sp => weekPlannerRepository.Object);
        Services.AddScoped(sp => userRepository.Object);

        return appState;
    }

    private AppState CreateAppStateWithLessonsPlanned()
    {
        var appState = CreateAppState();
        appState.CurrentYearData!.AddWeekPlanner(CreateWeekPlanner(appState.CurrentYearData!));
        return appState;
    }

    private static WeekPlanner CreateWeekPlanner(YearData yearData)
    {
        var weekPlanner = new WeekPlanner(yearData, DateTime.Now.Year, 1, 1, new DateOnly());

        var dayPlans = Enumerable.Range(0, 5)
            .Select(i => new DayPlan(weekPlanner.Id, new DateOnly().AddDays(i), CreateLessonPlans(new DateOnly().AddDays(i), yearData), [])).ToList();

        foreach (var dayPlan in dayPlans)
        {
            weekPlanner.UpdateDayPlan(dayPlan);
        }

        return weekPlanner;
    }

    private static List<LessonPlan> CreateLessonPlans(DateOnly date, YearData yearData)
    {
        return
        [
            new LessonPlan(yearData, new Subject([], "English"), PeriodType.Lesson, "", 1, 1, date,[]),
            new LessonPlan(yearData, new Subject([], "Mathematics"), PeriodType.Lesson, "", 1, 2, date, []),
            new LessonPlan(yearData, new Subject([], "Health and PE"), PeriodType.Lesson, "", 1, 4, date, []),
            new LessonPlan(yearData, new Subject([], "HASS"), PeriodType.Lesson, "", 1, 5, date, []),
            new LessonPlan(yearData, new Subject([], "Science"), PeriodType.Lesson, "", 1, 7, date, []),
            new LessonPlan(yearData, new Subject([], "Japanese"), PeriodType.Lesson, "", 1, 8, date, [])
        ];
    }

    private static WeekPlannerTemplate CreateWeekPlannerTemplateWithMultiPeriodLessons()
    {
        var periods = new List<TemplatePeriod>
        {
            new TemplatePeriod(PeriodType.Lesson, 1, "Lesson 1", new TimeOnly(09, 10, 0), new TimeOnly(10, 00, 0)),
            new TemplatePeriod(PeriodType.Lesson, 2, "Lesson 2", new TimeOnly(10, 00, 0), new TimeOnly(10, 50, 0)),
            new TemplatePeriod(PeriodType.Break, 3, "Recess", new TimeOnly(10, 50, 0), new TimeOnly(11, 20, 0)),
            new TemplatePeriod(PeriodType.Lesson, 4, "Lesson 3", new TimeOnly(11, 20, 0), new TimeOnly(12, 10, 0)),
            new TemplatePeriod(PeriodType.Lesson, 5, "Lesson 4", new TimeOnly(12, 10, 0), new TimeOnly(13, 00, 0)),
            new TemplatePeriod(PeriodType.Break, 6, "Lunch", new TimeOnly(13, 0, 0), new TimeOnly(13, 30, 0)),
            new TemplatePeriod(PeriodType.Lesson,7, "Lesson 5", new TimeOnly(13, 30, 0), new TimeOnly(14, 20, 0)),
            new TemplatePeriod(PeriodType.Lesson, 8,"Lesson 6", new TimeOnly(14, 20, 0), new TimeOnly(15, 10, 0))
        };
        var dayTemplates = new List<DayTemplate>();
        foreach (var day in Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday))
        {
            dayTemplates.Add(new DayTemplate(periods.Select<TemplatePeriod, PeriodBase>((p, i) =>
            {
                if (p.PeriodType == PeriodType.Lesson)
                {
                    return new LessonPeriod(p.Name ?? string.Empty, i + 1, 1);
                }
                else
                {
                    return new BreakPeriod(p.Name ?? string.Empty, i + 1, 1);
                }
            }).ToList(), day, DayType.Working));
        }

        dayTemplates[0].Periods.RemoveAt(1);
        dayTemplates[0].Periods[0].NumberOfPeriods = 2;
        dayTemplates[0].Periods.RemoveAt(3);
        dayTemplates[0].Periods[2].NumberOfPeriods = 2;

        var template = new WeekPlannerTemplate(periods, dayTemplates, Guid.NewGuid());
        return template;
    }
}
