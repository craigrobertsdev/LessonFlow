using Bunit;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using static LessonFlow.UnitTests.UnitTestHelpers;

namespace LessonFlow.UnitTests.UI.WeekPlannerTests;
public class WeekPlannerPageTests : TestContext
{
    private readonly AppState _appState;

    public WeekPlannerPageTests()
    {
        _appState = CreateAppState(2025);
        JSInterop.SetupVoid("Radzen.preventArrows", _ => true);
    }

    [Fact]
    public void InitialiseGrid_WhenNoLessonsPlanned_ShouldRenderFromWeekPlannerTemplate()
    {
        var component = RenderWeekPlannerPage(_appState);

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(8, col.Cells.Count);
            Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);
            Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
            Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
            Assert.Equal((6, 7), col.Cells[3].RowSpans[0]);
            Assert.Equal((7, 8), col.Cells[4].RowSpans[0]);
            Assert.Equal((8, 9), col.Cells[5].RowSpans[0]);
            Assert.Equal((9, 10), col.Cells[6].RowSpans[0]);
            Assert.Equal((10, 11), col.Cells[7].RowSpans[0]);
        }
    }

    [Fact]
    public void InitialiseGrid_WhenNitLessonsInWeekPlannerTemplateAndNoLessonsPlanned_ShouldRenderCorrectly()
    {
        var appState = CreateAppState(TestYear);
        var nitLesson = new NitTemplate(5, 2);
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[0].Periods[4] = nitLesson;
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[0].Periods.RemoveAt(6);
        var component = RenderWeekPlannerPage(appState);

        var cells = component.Instance.GridCols[0].Cells;

        Assert.Equal(7, cells.Count);
        Assert.Equal(typeof(LessonTemplate), cells[0].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), cells[1].Period.GetType());
        Assert.Equal(typeof(BreakTemplate), cells[2].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), cells[3].Period.GetType());
        Assert.Equal(typeof(NitTemplate), cells[4].Period.GetType());
        Assert.Equal(typeof(BreakTemplate), cells[5].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), cells[6].Period.GetType());
    }

    [Fact]
    public void InitialiseGrid_WhenAllLessonsPlanned_ShouldRenderFromDayPlan()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYear = 2025;
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
        var component = RenderWeekPlannerPage(appState);

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(8, col.Cells.Count);
            Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);
            Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
            Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
            Assert.Equal((6, 7), col.Cells[3].RowSpans[0]);
            Assert.Equal((7, 8), col.Cells[4].RowSpans[0]);
            Assert.Equal((8, 9), col.Cells[5].RowSpans[0]);
            Assert.Equal((9, 10), col.Cells[6].RowSpans[0]);
            Assert.Equal((10, 11), col.Cells[7].RowSpans[0]);
        }

        foreach (var col in component.Instance.GridCols)
        {
            Assert.Equal(typeof(LessonPlan), col.Cells[0].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[1].Period.GetType());
            Assert.Equal(typeof(BreakTemplate), col.Cells[2].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[3].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[4].Period.GetType());
            Assert.Equal(typeof(BreakTemplate), col.Cells[5].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[6].Period.GetType());
            Assert.Equal(typeof(LessonPlan), col.Cells[7].Period.GetType());
        }
    }

    [Fact]
    public void InitialiseGrid_WhenWeekPlannerTemplateHasPeriodOverlappingBreak_ShouldRenderCorrectly()
    {
        var appState = CreateAppState(TestYear);
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[0].Periods[4].NumberOfPeriods = 2;
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[0].Periods.RemoveAt(6);

        var component = RenderWeekPlannerPage(appState);
        var col = component.Instance.GridCols[0];
        Assert.Equal(2, col.Cells[4].RowSpans.Count);
        Assert.Equal([(7, 8), (9, 10)], component.Instance.GridCols[0].Cells[4].RowSpans);

        Assert.Equal(typeof(LessonTemplate), col.Cells[0].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), col.Cells[1].Period.GetType());
        Assert.Equal(typeof(BreakTemplate), col.Cells[2].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), col.Cells[3].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), col.Cells[4].Period.GetType());
        Assert.Equal(2, col.Cells[4].Period.NumberOfPeriods);
        Assert.Equal(typeof(BreakTemplate), col.Cells[5].Period.GetType());
        Assert.Equal(typeof(LessonTemplate), col.Cells[6].Period.GetType());
    }

    [Fact]
    public void InitialiseGrid_WhenSomeLessonsPlanned_ShouldRenderFromDayPlan()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].LessonPlans.RemoveAt(0);
        appState.CurrentYear = 2025;
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
        var component = RenderWeekPlannerPage(appState);

        var col = component.Instance.GridCols[0];
        Assert.Equal(8, col.Cells.Count);
        Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);
        Assert.Equal((4, 5), col.Cells[1].RowSpans[0]);
        Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
        Assert.Equal((6, 7), col.Cells[3].RowSpans[0]);
        Assert.Equal((7, 8), col.Cells[4].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[5].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[6].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[7].RowSpans[0]);

        Assert.Equal(typeof(LessonTemplate), col.Cells[0].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[1].Period.GetType());
        Assert.Equal(typeof(BreakTemplate), col.Cells[2].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[3].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[4].Period.GetType());
        Assert.Equal(typeof(BreakTemplate), col.Cells[5].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[6].Period.GetType());
        Assert.Equal(typeof(LessonPlan), col.Cells[7].Period.GetType());
    }

    [Fact]
    public void InitialiseGrid_WhenDayPlanHasMultiPeriodLessons_ShouldRenderCorrectly()
    {
        var appState = CreateAppStateWithMultiPeriodLessons();
        var component = RenderWeekPlannerPage(appState);

        var col = component.Instance.GridCols[0];
        Assert.Equal(6, col.Cells.Count);
        Assert.Equal((3, 4), col.Cells[0].RowSpans[0]);
        Assert.Equal([(4, 5), (6, 7)], col.Cells[1].RowSpans);
        Assert.Equal((5, 6), col.Cells[2].RowSpans[0]);
        Assert.Equal([(7, 8), (9, 10)], col.Cells[3].RowSpans);
        Assert.Equal((8, 9), col.Cells[4].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[5].RowSpans[0]);
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
        appState.CurrentYear = 2025;
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
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
        Assert.Equal((3, 5), col.Cells[0].RowSpans[0]);

        Assert.Equal((5, 6), col.Cells[1].RowSpans[0]);
        Assert.Single(col.Cells[1].RowSpans);

        Assert.Equal((6, 8), col.Cells[2].RowSpans[0]);
        Assert.Equal((8, 9), col.Cells[3].RowSpans[0]);
        Assert.Equal((9, 10), col.Cells[4].RowSpans[0]);
        Assert.Equal((10, 11), col.Cells[5].RowSpans[0]);
    }

    [Fact]
    public void NavigateToNextWeek_WhenNextWeekWithinSameYear_LoadsNextWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYear = 2025;
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
        var component = RenderWeekPlannerPage(appState);
        component.Find("button#next-week").Click();

        Assert.Equal(2, component.Instance.AppState.CurrentWeek);
        Assert.Equal(1, component.Instance.AppState.CurrentTerm);
        Assert.Equal(2, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(1, component.Instance.WeekPlanner.TermNumber);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 2 && wp.TermNumber == 1));
        Assert.Equal(2025, component.Instance.AppState.CurrentYearData!.CalendarYear);
    }

    [Theory]
    [MemberData(nameof(NavigateToNextWeekAfterHolidaysGenerator))]
    public void NavigateToNextWeek_WhenNextWeekAfterSchoolHoliday_LoadsFirstWeekPlannerOfNextTerm(int year, int term, int week, int expectedYear, int expectedTerm, int expectedWeek)
    {
        var component = RenderWeekPlannerPage(_appState);
        component.Instance.AppState.CurrentYear = year;
        component.Instance.AppState.CurrentTerm = term;
        component.Instance.AppState.CurrentWeek = week;

        component.Find("button#next-week").Click();
        Assert.Equal(expectedYear, component.Instance.AppState.CurrentYear);
        Assert.Equal(expectedTerm, component.Instance.AppState.CurrentTerm);
        Assert.Equal(expectedWeek, component.Instance.AppState.CurrentWeek);
        Assert.Equal(expectedWeek, component.Instance.WeekPlanner.WeekNumber);
    }

    [Theory]
    [MemberData(nameof(NavigateToNextWeekAfterHolidaysGenerator))]
    public void NavigateToNextWeek_NavigateFirstThenWhenNextWeekAfterSchoolHoliday_LoadsFirstWeekPlannerOfNextTerm(int year, int term, int week, int expectedYear, int expectedTerm, int expectedWeek)
    {
        var component = RenderWeekPlannerPage(_appState);
        component.Instance.AppState.CurrentYear = year;
        component.Instance.AppState.CurrentTerm = term;
        component.Instance.AppState.CurrentWeek = week;

        component.Find("button#previous-week").Click();
        var nextButton = component.Find("button#next-week");
        nextButton.Click();
        nextButton.Click();
        Assert.Equal(expectedYear, component.Instance.AppState.CurrentYear);
        Assert.Equal(expectedTerm, component.Instance.AppState.CurrentTerm);
        Assert.Equal(expectedWeek, component.Instance.AppState.CurrentWeek);
        Assert.Equal(expectedWeek, component.Instance.WeekPlanner.WeekNumber);
    }

    [Fact]
    public void NavigateToNextWeek_WhenNextWeekIsNextYear_LoadsNextWeekPlannerWithNextYearData()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYear = 2025;
        appState.CurrentTerm = 4;
        appState.CurrentWeek = 9;
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#next-week").Click();

        Assert.Equal(2026, component.Instance.AppState.CurrentYear);
        Assert.Equal(2026, component.Instance.AppState.CurrentYearData!.CalendarYear);

        Assert.Equal(1, component.Instance.AppState.CurrentTerm);
        Assert.Equal(1, component.Instance.WeekPlanner.TermNumber);

        Assert.Equal(1, component.Instance.AppState.CurrentWeek);
        Assert.Equal(1, component.Instance.WeekPlanner.WeekNumber);

        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 1 && wp.TermNumber == 1));
    }

    [Theory]
    [InlineData(2026, 4, 8, true)]
    [InlineData(2025, 4, 8, false)]
    public void NavigateToNextWeek_WhenOutOfRange_ButtonCannotBeClicked(int year, int termNumber, int weekNumber, bool isDisabled)
    {
        var appState = CreateAppState(year);
        appState.CurrentTerm = termNumber;
        appState.CurrentWeek = weekNumber;
        var component = RenderWeekPlannerPage(appState);

        var nextWeekButton = component.Find("button#next-week");
        nextWeekButton.Click();

        Assert.Equal(isDisabled, nextWeekButton.HasAttribute("disabled"));
    }

    [Fact]
    public void NavigateToPreviousWeek_WhenPreviousWeekWithinSameYear_LoadsPreviousWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        appState.CurrentYear = 2025;
        appState.CurrentWeek = 2;
        appState.CurrentTerm = 1;
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#previous-week").Click();

        Assert.Equal(1, component.Instance.AppState.CurrentWeek);
    }

    [Theory]
    [MemberData(nameof(NavigateToLastWeekBeforeHolidaysGenerator))]
    public void NavigateToPreviousWeek_WhenPreviousWeekBeforeSchoolHolidays_LoadsLastWeekOfPreviousTerm(int year, int term, int week, int expectedYear, int expectedTerm, int expectedWeek)
    {
        var appState = CreateAppState(year);
        appState.CurrentTerm = term;
        appState.CurrentWeek = week;
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#previous-week").Click();

        Assert.Equal(expectedYear, component.Instance.AppState.CurrentYear);
        Assert.Equal(expectedTerm, component.Instance.AppState.CurrentTerm);
        Assert.Equal(expectedWeek, component.Instance.AppState.CurrentWeek);
        Assert.Equal(expectedWeek, component.Instance.WeekPlanner.WeekNumber);
    }

    [Fact]
    public void NavigateToPreviousWeek_WhenPreviousWeekIsPreviousYear_LoadsPreviousWeekPlannerWithPreviousYearData()
    {
        var appState = CreateAppState(2026);
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#previous-week").Click();

        Assert.Equal(9, component.Instance.AppState.CurrentWeek);
        Assert.Equal(9, component.Instance.WeekNumber);
        Assert.Equal(9, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(4, component.Instance.AppState.CurrentTerm);
        Assert.Equal(4, component.Instance.TermNumber);
        Assert.Equal(4, component.Instance.WeekPlanner.TermNumber);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 9 && wp.TermNumber == 4));
        Assert.Equal(2025, component.Instance.AppState.CurrentYear);
        Assert.Equal(2025, component.Instance.Year);
        Assert.Equal(2025, component.Instance.AppState.CurrentYearData!.CalendarYear);
    }

    [Fact]
    public void NavigateToPreviousWeekAfterNavigatingToNextWeek_PreviousWeekIsPreviousYear_LoadsPreviousWeekPlannerWithPreviousYearData()
    {
        var appState = CreateAppState(2025);
        appState.CurrentTerm = 4;
        appState.CurrentWeek = 9;
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#next-week").Click();
        component.Find("button#previous-week").Click();

        Assert.Equal(9, component.Instance.SelectedWeek);
        Assert.Equal(9, component.Instance.AppState.CurrentWeek);
        Assert.Equal(9, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(4, component.Instance.SelectedTerm);
        Assert.Equal(4, component.Instance.AppState.CurrentTerm);
        Assert.Equal(4, component.Instance.WeekPlanner.TermNumber);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == 9 && wp.TermNumber == 4));
        Assert.Equal(2025, component.Instance.AppState.CurrentYear);
        Assert.Equal(2025, component.Instance.Year);
        Assert.Equal(2025, component.Instance.AppState.CurrentYearData!.CalendarYear);
    }

    [Theory]
    [InlineData(2026, 1, 2, false)]
    [InlineData(2025, 1, 2, true)]
    public void NavigateToPreviousWeek_WhenOutOfRange_ButtonCannotBeClicked(int year, int termNumber, int weekNumber, bool isDisabled)
    {
        var appState = CreateAppState(year);
        appState.CurrentTerm = termNumber;
        appState.CurrentWeek = weekNumber;
        var component = RenderWeekPlannerPage(appState);

        var previousWeekButton = component.Find("button#previous-week");
        previousWeekButton.Click();

        Assert.Equal(isDisabled, previousWeekButton.HasAttribute("disabled"));
    }

    [Theory]
    [MemberData(nameof(GoToSelectedWeekDatesGenerator))]
    public void GoToSelectedWeek_WhenInTermTime_LoadsCorrectWeekPlanner(int year, int termNumber, int weekNumber)
    {
        var component = RenderWeekPlannerPage(_appState);
        component.Instance.SelectedYear = year;
        component.Instance.SelectedTerm = termNumber;
        component.Instance.SelectedWeek = weekNumber;

        component.Find("select#year-selector").Change(year.ToString());
        component.Find("select#term-selector").Change(termNumber.ToString());
        component.Find("select#week-selector").Change(weekNumber.ToString());
        component.Find("button#go-to-week").Click();

        Assert.Equal(year, component.Instance.SelectedYear);
        Assert.Equal(year, component.Instance.AppState.CurrentYear);

        Assert.Equal(termNumber, component.Instance.SelectedTerm);
        Assert.Equal(termNumber, component.Instance.WeekPlanner.TermNumber);
        Assert.Equal(termNumber, component.Instance.AppState.CurrentTerm);

        Assert.Equal(weekNumber, component.Instance.SelectedWeek);
        Assert.Equal(weekNumber, component.Instance.WeekPlanner.WeekNumber);
        Assert.Equal(weekNumber, component.Instance.AppState.CurrentWeek);
        Assert.NotNull(component.Instance.AppState.CurrentYearData!.WeekPlanners.FirstOrDefault(wp => wp.WeekNumber == weekNumber && wp.TermNumber == termNumber));
    }

    [Fact]
    public void EditWeekPlanner_WhenClicked_CreatesACopyOfCurrentWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var component = RenderWeekPlannerPage(appState);

        component.Find("button#edit-week-planner").Click();

        var weekPlanner = component.Instance.WeekPlanner;
        var editingWeekPlanner = component.Instance.EditingWeekPlanner;

        Assert.NotNull(editingWeekPlanner);
        for (int i = 0; i < weekPlanner.DayPlans.Count; i++)
        {
            for (int j = 0; j < weekPlanner.DayPlans[i].LessonPlans.Count; j++)
            {
                Assert.Equal(weekPlanner.DayPlans[i].LessonPlans[j], editingWeekPlanner.DayPlans[i].LessonPlans[j]);
            }

            for (int j = 0; j < weekPlanner.DayPlans[i].SchoolEvents.Count; j++)
            {
                Assert.Equal(weekPlanner.DayPlans[i].SchoolEvents[j], editingWeekPlanner.DayPlans[i].SchoolEvents[j]);
            }

            Assert.Equal(weekPlanner.DayPlans[i].BreakDutyOverrides, editingWeekPlanner.DayPlans[i].BreakDutyOverrides);
            Assert.Equal(weekPlanner.DayPlans[i].DayOfWeek, editingWeekPlanner.DayPlans[i].DayOfWeek);
        }

        Assert.NotEqual(weekPlanner.Id, editingWeekPlanner.Id);
    }

    [Fact]
    public void EditWeekPlanner_WhenCancelClicked_DiscardsTheEditingWeekPlanner()
    {
        var appState = CreateAppStateWithLessonsPlanned();
        var component = RenderWeekPlannerPage(appState);
        var weekPlanner = component.Instance.WeekPlanner;

        component.Find("button#edit-week-planner").Click();
        Assert.NotNull(component.Instance.EditingWeekPlanner);

        component.Find("button#cancel-editing").Click();
        Assert.Null(component.Instance.EditingWeekPlanner);
        Assert.Equal(weekPlanner, component.Instance.WeekPlanner);
    }

    [Fact]
    public void EditWeekPlanner_WhenBreakDutyAddedInInputField_UpdatesEditingWeekPlanner()
    {
        var component = RenderWeekPlannerPage(CreateAppStateWithLessonsPlanned());
        component.Find("button#edit-week-planner").Click();
        var editingWeekPlanner = component.Instance.EditingWeekPlanner;
        Assert.NotNull(editingWeekPlanner);

        var breakDutyInput = component.Find("input#break-duty-2-3");
        breakDutyInput.Change("Test Duty");

        Assert.Equal("Test Duty", component.Instance.EditingWeekPlanner!.DayPlans[0].BreakDutyOverrides[3]);
    }

    [Fact]
    public async Task SaveChanges_WhenClicked_UpdatesFieldsInComponentWeekPlanner()
    {
        var component = RenderWeekPlannerPage(CreateAppStateWithLessonsPlanned());
        var weekPlanner = component.Instance.WeekPlanner;

        component.Find("button#edit-week-planner").Click();
        var breakDutyInput = component.Find("input#break-duty-2-3");
        breakDutyInput.Change("Test Duty");

        var saveButton = component.Find("button#save-changes");
        await saveButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        var editingWeekPlanner = component.Instance.EditingWeekPlanner;
        Assert.Null(editingWeekPlanner);
        Assert.Equal(weekPlanner.Id, component.Instance.WeekPlanner.Id);
        Assert.Equal("Test Duty", component.Instance.WeekPlanner.DayPlans[0].BreakDutyOverrides[3]);
    }

    [Fact]
    public async Task SaveChanges_WhenChangingExistingBreakDutyOverride_ShouldUpdateCorrectly()
    {
        var originalValue = "Test Duty";
        var newValue = "Test Duty 2";
        var component = RenderWeekPlannerPage(CreateAppStateWithLessonsPlanned());
        var weekPlanner = component.Instance.WeekPlanner;

        component.Find("button#edit-week-planner").Click();
        component.Find("input#break-duty-2-3").Change(originalValue);
        await component.Find("button#save-changes").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        component.Find("button#edit-week-planner").Click();
        component.Find("input#break-duty-2-3").Change(newValue);
        await component.Find("button#save-changes").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        var editingWeekPlanner = component.Instance.EditingWeekPlanner;
        Assert.Null(editingWeekPlanner);
        Assert.Equal(weekPlanner.Id, component.Instance.WeekPlanner.Id);
        Assert.Equal(newValue, component.Instance.WeekPlanner.DayPlans[0].BreakDutyOverrides[3]);
    }

    [Fact]
    public async Task SaveChanges_WhenClearingExistingBreakDutyOverride_ShouldRemoveFromOverrides()
    {
        var originalValue = "Test Duty";
        var newValue = string.Empty;
        var component = RenderWeekPlannerPage(CreateAppStateWithLessonsPlanned());
        var weekPlanner = component.Instance.WeekPlanner;

        component.Find("button#edit-week-planner").Click();
        component.Find("input#break-duty-2-3").Change(originalValue);
        await component.Find("button#save-changes").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        component.Find("button#edit-week-planner").Click();
        component.Find("input#break-duty-2-3").Change(newValue);
        await component.Find("button#save-changes").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        var editingWeekPlanner = component.Instance.EditingWeekPlanner;
        Assert.Null(editingWeekPlanner);
        Assert.Equal(weekPlanner.Id, component.Instance.WeekPlanner.Id);
        Assert.False(weekPlanner.DayPlans[0].BreakDutyOverrides.ContainsKey(3));
    }

    public static TheoryData<int, int, int> GoToSelectedWeekDatesGenerator()
    {
        var data = new TheoryData<int, int, int>();
        data.Add(2025, 1, 11);
        data.Add(2025, 2, 10);
        data.Add(2025, 3, 10);
        data.Add(2025, 4, 7);
        data.Add(2026, 4, 7);
        return data;
    }

    public static TheoryData<int, int, int, int, int, int> NavigateToNextWeekAfterHolidaysGenerator()
    {
        var data = new TheoryData<int, int, int, int, int, int>();
        data.Add(2025, 1, 11, 2025, 2, 1);
        data.Add(2025, 2, 10, 2025, 3, 1);
        data.Add(2025, 3, 10, 2025, 4, 1);

        return data;
    }

    public static TheoryData<int, int, int, int, int, int> NavigateToLastWeekBeforeHolidaysGenerator()
    {
        var data = new TheoryData<int, int, int, int, int, int>();
        data.Add(2025, 2, 1, 2025, 1, 11);
        data.Add(2025, 3, 1, 2025, 2, 10);
        data.Add(2025, 4, 1, 2025, 3, 10);

        return data;
    }

    private IRenderedComponent<WeekPlannerPage> RenderWeekPlannerPage(AppState appState)
    {
        var component = base.RenderComponent<WeekPlannerPage>(p => p.Add(c => c.AppState, appState));
        return component;
    }

    private AppState CreateAppState(int calendarYear)
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var userRepository = new Mock<IUserRepository>();
        var weekPlannerRepository = new Mock<IWeekPlannerRepository>();
        var yearDataRepository = new Mock<IYearDataRepository>();
        var logger = new Mock<ILogger<AppState>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var termDatesService = UnitTestHelpers.CreateTermDatesService();

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        appState.CurrentYear = calendarYear;

        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(calendarYear);

        var yearData = new YearData(Guid.NewGuid(), accountSetupState);
        var weekPlannerTemplate = UnitTestHelpers.GenerateWeekPlannerTemplate();

        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearDataByYear.Add(yearData.CalendarYear, yearData);
        appState.User = new User() { AccountSetupComplete = true };

        var weekPlanner = new WeekPlanner(yearData, 2025, 3, 2, new DateOnly(2025, 7, 28));
        weekPlannerRepository.Setup(wp => wp.GetWeekPlanner(yearData.Id, 2025, 3, 2, new CancellationToken()).Result).Returns(weekPlanner);

        Services.AddScoped(sp => termDatesService);
        Services.AddScoped(sp => weekPlannerRepository.Object);
        Services.AddScoped(sp => userRepository.Object);
        Services.AddScoped(sp => yearDataRepository.Object);
        Services.AddScoped(sp => unitOfWork.Object);

        appState.GetType().GetProperty(nameof(appState.IsInitialised))!.SetValue(appState, true);
        return appState;
    }

    private AppState CreateAppStateWithLessonsPlanned()
    {
        var appState = CreateAppState(2025);
        appState.CurrentYearData!.AddWeekPlanner(CreateWeekPlanner(appState.CurrentYearData!));
        appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].SchoolEvents = CreateSchoolEvents(appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].Date);
        return appState;
    }

    private static WeekPlanner CreateWeekPlanner(YearData yearData)
    {
        var weekPlanner = new WeekPlanner(yearData, TestYear, 1, 1, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool));
        yearData.WeekPlanners.Add(weekPlanner);
        var dayPlans = Enumerable.Range(0, 5)
            .Select(i => new DayPlan(weekPlanner.Id, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool).AddDays(i), CreateLessonPlans(new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool).AddDays(i), yearData), [])).ToList();

        foreach (var dayPlan in dayPlans)
        {
            weekPlanner.UpdateDayPlan(dayPlan);
        }

        return weekPlanner;
    }

    private AppState CreateAppStateWithMultiPeriodLessons()
    {
        var appState = CreateAppState(TestYear);
        appState.CurrentYearData!.AddWeekPlanner(CreateWeekPlannerWithMultiPeriodLessons(appState.CurrentYearData!));
        appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].SchoolEvents = CreateSchoolEvents(appState.CurrentYearData!.WeekPlanners[0].DayPlans[0].Date);
        return appState;
    }

    private static WeekPlanner CreateWeekPlannerWithMultiPeriodLessons(YearData yearData)
    {
        var weekPlanner = new WeekPlanner(yearData, TestYear, 1, 1, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool));
        yearData.WeekPlanners.Add(weekPlanner);
        var dayPlans = Enumerable.Range(0, 5)
            .Select(i => new DayPlan(weekPlanner.Id, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool).AddDays(i), CreateMultiPeriodLessonPlans(new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool).AddDays(i), yearData), [])).ToList();
        foreach (var dayPlan in dayPlans)
        {
            weekPlanner.UpdateDayPlan(dayPlan);
        }
        return weekPlanner;

        static List<LessonPlan> CreateMultiPeriodLessonPlans(DateOnly date, YearData yearData)
        {
            var dayPlan = yearData.WeekPlanners.First().DayPlans.First(dp => dp.Date == date);
            return
            [
                new LessonPlan(dayPlan.Id, new Subject([], "English"), PeriodType.Lesson, "", 1, 1, date,[]),
                new LessonPlan(dayPlan.Id, new Subject([], "Mathematics"), PeriodType.Lesson, "", 2, 2, date, []),
                new LessonPlan(dayPlan.Id, new Subject([], "HASS"), PeriodType.Lesson, "", 2, 5, date, []),
                new LessonPlan(dayPlan.Id, new Subject([], "Japanese"), PeriodType.Lesson, "", 1, 8, date, [])
            ];
        }
    }

    private static List<LessonPlan> CreateLessonPlans(DateOnly date, YearData yearData)
    {
        var dayPlan = yearData.WeekPlanners.First().DayPlans.First(dp => dp.Date == date);
        return
        [
            new LessonPlan(dayPlan.Id, new Subject([], "English"), PeriodType.Lesson, "", 1, 1, date,[]),
            new LessonPlan(dayPlan.Id, new Subject([], "Mathematics"), PeriodType.Lesson, "", 1, 2, date, []),
            new LessonPlan(dayPlan.Id, new Subject([], "Health and PE"), PeriodType.Lesson, "", 1, 4, date, []),
            new LessonPlan(dayPlan.Id, new Subject([], "HASS"), PeriodType.Lesson, "", 1, 5, date, []),
            new LessonPlan(dayPlan.Id, new Subject([], "Science"), PeriodType.Lesson, "", 1, 7, date, []),
            new LessonPlan(dayPlan.Id, new Subject([], "Japanese"), PeriodType.Lesson, "", 1, 8, date, [])
        ];
    }

    private static List<SchoolEvent> CreateSchoolEvents(DateOnly date)
    {
        return
        [
            new SchoolEvent(new SchoolEventId(Guid.NewGuid()), new Location("123", "Fake", "DisneyLand"), "Swimming", true, new DateTime(date, new TimeOnly()), new DateTime(date, new TimeOnly())),
            new SchoolEvent(new SchoolEventId(Guid.NewGuid()), new Location("456", "Main St", "FakeLand"), "Athletics", false, new DateTime(date, new TimeOnly()), new DateTime(date, new TimeOnly())),
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
            dayTemplates.Add(new DayTemplate(periods.Select<TemplatePeriod, PeriodTemplateBase>((p, i) =>
            {
                if (p.PeriodType == PeriodType.Lesson)
                {
                    return new LessonTemplate(p.Name ?? string.Empty, i + 1, 1);
                }
                else
                {
                    return new BreakTemplate(p.Name ?? string.Empty, i + 1, 1);
                }
            }).ToList(), day, DayType.Working));
        }

        dayTemplates[0].Periods.RemoveAt(1);
        dayTemplates[0].Periods[0].NumberOfPeriods = 2;
        dayTemplates[0].Periods.RemoveAt(3);
        dayTemplates[0].Periods[2].NumberOfPeriods = 2;

        var template = new WeekPlannerTemplate(Guid.NewGuid(), periods, dayTemplates);
        return template;
    }
}
