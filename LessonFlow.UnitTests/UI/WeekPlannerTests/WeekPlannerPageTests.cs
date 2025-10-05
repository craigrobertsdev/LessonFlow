using Bunit;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;

namespace LessonFlow.UnitTests.UI.WeekPlannerTests;
public class WeekPlannerPageTests : TestContext
{
    [Fact]
    public void InitialiseGrid_WhenNoLessonsPlanned_ShouldRenderFromWeekPlannerTemplate()
    {
        var component = RenderWeekPlannerPage();

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
        var component = RenderWeekPlannerPage();
        var weekPlanner = CreateWeekPlanner(component.Instance.YearData);
        component.Instance.YearData!.AddWeekPlanner(weekPlanner);

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
            foreach (var cell in col.Cells)
            {
                // Do assertion to confirm that the period is a LessonPlan not a LessonPeriod
            }
        }
    }

    [Fact]
    public void SetRowSpans_WithMultiPeriodLessonAfterFirstBreak_ShouldSetCorrectly()
    {
        var component = RenderWeekPlannerPage();

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

    private IRenderedComponent<WeekPlannerPage> RenderWeekPlannerPage()
    {
        var appState = CreateAppState();
        var component = base.RenderComponent<WeekPlannerPage>(p => p.Add(c => c.AppState, appState));
        return component;
    }

    private static AppState CreateAppState()
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var userRepository = new Mock<IUserRepository>();
        var logger = new Mock<ILogger<AppState>>();

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        var yearData = new YearData(Guid.NewGuid(), new AccountSetupState(Guid.NewGuid()));
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();
        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearData = yearData;

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
            new LessonPlan(yearData, new Subject([], "Health and PE"), PeriodType.Lesson, "", 1, 3, date, []),
            new LessonPlan(yearData, new Subject([], "HASS"), PeriodType.Lesson, "", 1, 4, date, []),
            new LessonPlan(yearData, new Subject([], "Science"), PeriodType.Lesson, "", 1, 5, date, []),
            new LessonPlan(yearData, new Subject([], "Japanese"), PeriodType.Lesson, "", 1, 6, date, [])
        ];
    }
}
