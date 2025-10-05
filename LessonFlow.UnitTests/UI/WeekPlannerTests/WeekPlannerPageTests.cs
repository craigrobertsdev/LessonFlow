using Bunit;
using LessonFlow.Api.Services;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Interfaces.Services;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace LessonFlow.UnitTests.UI.WeekPlannerTests;
public class WeekPlannerPageTests : TestContext
{
    // TODO: Write a test to handle non-working days at the start, end and in the middle of the week

    [Fact]
    public void InitialiseGrid_WhenNoLessonsPlanned_ShouldRenderFromWeekPlannerTemplate()
    {
        var appState = CreateAppState();
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
            { "year", DateTime.Now.Year.ToString() }
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
    public void SetRowSpans_WithMultiPeriodLessonAfterFirstBreak_ShouldSetCorrectly()
    {
        var appState = CreateAppState();
        var component = RenderWeekPlannerPage(appState);

        var periods = component.Instance.AppState.YearData!.WeekPlannerTemplate.DayTemplates[0].Periods;
        periods[0].NumberOfPeriods = 2;
        periods.RemoveAt(1);
        periods[2].NumberOfPeriods = 2;
        periods.RemoveAt(3);

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

    private IRenderedComponent<WeekPlannerPage> RenderWeekPlannerPage(AppState appState)
    {
        var component = base.RenderComponent<WeekPlannerPage>(p => p.Add(c => c.AppState, appState));
        return component;
    }

    private AppState CreateAppState()
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var userRepository = new Mock<IUserRepository>();
        var logger = new Mock<ILogger<AppState>>();
        var termDatesService = new Mock<ITermDatesService>();
        Services.AddScoped(sp => termDatesService.Object);

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        var yearData = new YearData(Guid.NewGuid(), new AccountSetupState(Guid.NewGuid()));
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearData = yearData;

        return appState;
    }

    private AppState CreateAppStateWithLessonsPlanned()
    {

        var appState = CreateAppState();
        appState.YearData!.AddWeekPlanner(CreateWeekPlanner(appState.YearData));
        return appState;
    }

    private static WeekPlanner CreateWeekPlanner(YearData yearData)
    {
        var weekPlanner = new WeekPlanner(yearData, 1, 1, DateTime.Now.Year, new DateOnly());

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
}
