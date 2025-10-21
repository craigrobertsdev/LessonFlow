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
using static LessonFlow.UnitTests.Helpers;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Radzen;

namespace LessonFlow.UnitTests.UI.LessonPlannerTests;
public class LessonPlannerTests : TestContext
{
    private readonly AppState _appState;

    public LessonPlannerTests()
    {
        _appState = CreateAppState();
        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);
    }

    [Fact]
    public void Initialise_WhenLessonPlanExists_ShouldInitialiseCorrectly()
    {
        var periodStart = 1;
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, periodStart);

        Assert.NotNull(component.Instance.LessonPlan);
        Assert.Equal(new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool), component.Instance.LessonPlan.LessonDate);
        Assert.Equal(periodStart, component.Instance.LessonPlan.StartPeriod);
        Assert.Equal("English", component.Instance.LessonPlan.Subject.Name);
        Assert.Single(component.Instance.LessonPlan.Resources);
        var textEditor = component.Find("#lesson-plan-editor");
        var todoList = component.Find("#lesson-plan-todo-list");
        Assert.NotNull(textEditor);
        Assert.NotNull(todoList);
    }

    [Fact]
    public void Initialise_WhenNoLessonPlanned_ShouldCreateNewLessonPlan()
    {
        var day = 30; // No lesson planned on this date in the mock setup
        var periodStart = 1;
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, day, periodStart);

        var lessonPlan = component.Instance.LessonPlan;

        Assert.NotNull(lessonPlan);
        Assert.Equal(new DateOnly(TestYear, FirstMonthOfSchool, day), lessonPlan.LessonDate);
        Assert.Equal(periodStart, lessonPlan.StartPeriod);
    }

    [Theory]
    [InlineData(1, 1, "Mathematics")]
    [InlineData(2, 2, "English")]
    public void Initialise_WhenNoLessonPlanned_ShouldCreateLessonPlanFromWeekPlannerTemplate(int periodStart, int numberOfPeriods, string subjectName)
    {

        var day = 30;
        var date = new DateOnly(TestYear, FirstMonthOfSchool, day);
        var appState = CreateAppState();
        var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(It.IsAny<YearDataId>(), It.IsAny<DateOnly>(), periodStart, default))
           .ReturnsAsync(new LessonPlan(appState.CurrentYearData, new Subject([], subjectName), PeriodType.Lesson, "", numberOfPeriods, periodStart, date, []));

        Services.AddScoped(sp => lessonPlanRepository.Object);
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[1].Periods[0] = new LessonPeriod(subjectName, periodStart, numberOfPeriods);
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, day, periodStart);
        // Act
        var lessonPlan = component.Instance.LessonPlan;
        // Assert
        Assert.NotNull(lessonPlan);
        Assert.Equal(date, lessonPlan.LessonDate);
        Assert.Equal(periodStart, lessonPlan.StartPeriod);
        Assert.Equal(subjectName, lessonPlan.Subject.Name);
        Assert.Equal(numberOfPeriods, lessonPlan.NumberOfPeriods);
    }

    [Fact]
    public void Initialise_WhenNoLessonPlannedAndNoLessonTemplate_ShouldCreateBlankLessonPlan()
    {
        var appState = CreateAppState();
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates.Clear();
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, 31, 1);

        var lessonPlan = component.Instance.LessonPlan;
        var curriculumService = Services.GetRequiredService<ICurriculumService>();
        Assert.NotNull(lessonPlan);
        Assert.Equal(curriculumService.CurriculumSubjects.First().Name, lessonPlan.SubjectName);
        Assert.Equal(1, lessonPlan.NumberOfPeriods);
        Assert.Equal(1, lessonPlan.StartPeriod);
    }

    [Fact]
    public void Initialise_WhenNoLessonPlannedAndNitPeriodInWeekPlannerTemplate_ShouldCreateBlankNitPeriod()
    {
        var appState = CreateAppState();
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[4].Periods[0] = new NitPeriod(1, 2);
        var day = 31;
        var periodStart = 1;
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, day, periodStart);

        var lessonPlan = component.Instance.LessonPlan;
        Assert.NotNull(lessonPlan);
        Assert.Equal(PeriodType.Nit, lessonPlan.PeriodType);
        Assert.Equal(Subject.Nit, lessonPlan.Subject);

        var nitEditor = component.Find("#nit-editor");
        var notesSection = component.Find("#nit-notes-section");
        var todoList = component.Find("#nit-todo-list");
        Assert.NotNull(nitEditor);
        Assert.NotNull(notesSection);
        Assert.NotNull(todoList);
    }

    [Fact]
    public void Initialise_WhenNitLesson_ShouldCorrectlyLoadNitLesson()
    {
        var appState = CreateAppState();

        var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        var lessonPlan = new LessonPlan(appState.CurrentYearData, Subject.Nit, PeriodType.Nit, "", 2, 1, new DateOnly(2025, 1, 31), []);
        var todoItem = new TodoItem(lessonPlan.Id, "Test");
        lessonPlan.ToDos.Add(todoItem);
        lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(It.IsAny<YearDataId>(), It.IsAny<DateOnly>(), 1, default))
           .ReturnsAsync(lessonPlan);
        Services.AddScoped(sp => lessonPlanRepository.Object);
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[4].Periods[0] = new NitPeriod(1, 2);

        var day = 31;
        var periodStart = 1;
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, day, periodStart);

        var componentLessonPlan = component.Instance.LessonPlan;
        Assert.NotNull(componentLessonPlan);
        Assert.Equal(PeriodType.Nit, componentLessonPlan.PeriodType);
        Assert.Equal(Subject.Nit, componentLessonPlan.Subject);
        Assert.Single(componentLessonPlan.ToDos);
    }

    /* Editing a lesson plan will require the user to click a button to enable editing.
     * If they change the number of periods, they may overwrite existing data.
     * These changes will need to be confirmed by the user.
     */
    [Fact]
    public void CanEditLessonPlan_WhenLessonPlanExists_ShouldNotLoadIntoEditMode()
    {
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);

        Assert.False(component.Instance.IsInEditMode);
        Assert.NotNull(component.Find("p#subject-name"));
        Assert.NotNull(component.Find("p#number-of-periods"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#number-of-periods"));
    }

    [Fact]
    public void CanEditLessonPlan_WhenNoLessonPlanExistsButSubjectPlannedInWeekPlannerTemplate_ShouldNotLoadIntoEditMode()
    {
        var appState = CreateAppState();
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[3].Periods[0] = new LessonPeriod("Mathematics", 1, 2);
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, 30, 1);

        var subjectName = component.Find("p#subject-name");
        var numberOfPeriods = component.Find("p#number-of-periods");

        Assert.False(component.Instance.IsInEditMode);
        Assert.NotNull(subjectName);
        Assert.NotNull(numberOfPeriods);
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#number-of-periods"));
    }

    [Fact]
    public void CanEditLessonPlan_WhenNoLessonPlanOrSubjectPlannedInWeekPlannerTemplate_ShouldLoadIntoEditMode()
    {
        var appState = CreateAppState();
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[4].Periods.Clear();
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, 31, 1);

        Assert.True(component.Instance.IsInEditMode);
        Assert.NotNull(component.Find("select#subject-name"));
        Assert.NotNull(component.Find("select#number-of-periods"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("p#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("p#number-of-periods"));
    }

    [Fact]
    public void EditingLessonPlan_ShouldAllowChangesToBeMade()
    {
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);
        component.Find("#edit-lesson-plan").Click();

        Assert.True(component.Instance.IsInEditMode);
        Assert.NotNull(component.Find("select#subject-name"));
        Assert.NotNull(component.Find("select#number-of-periods"));
        Assert.NotNull(component.Find("#save-lesson-plan"));
        Assert.NotNull(component.Find("#cancel-editing"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("p#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("p#number-of-periods"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("#edit-lesson-plan"));

        var lessonPlan = component.Instance.LessonPlan;
        var editingLessonPlan = component.Instance.EditingLessonPlan;
        Assert.NotNull(editingLessonPlan);

        var type = lessonPlan.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.Name == nameof(LessonPlan.CreatedDateTime) || property.Name == nameof(LessonPlan.UpdatedDateTime))
            {
                continue;
            }

            var lessonValue = property.GetValue(lessonPlan);
            var editingValue = property.GetValue(editingLessonPlan);
            Assert.Equal(lessonValue, editingValue);
        }
    }

    [Fact]
    public void CancelEditing_ShouldMakeNoChangesAndRevertToViewMode()
    {
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);
        component.Find("#edit-lesson-plan").Click();
        component.Find("#cancel-editing").Click();
        var createdTime = component.Instance.LessonPlan.CreatedDateTime;
        var updatedTime = component.Instance.LessonPlan.UpdatedDateTime;

        Assert.False(component.Instance.IsInEditMode);
        Assert.NotNull(component.Find("p#subject-name"));
        Assert.NotNull(component.Find("p#number-of-periods"));
        Assert.NotNull(component.Find("#edit-lesson-plan"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("#save-lesson-plan"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("#cancel-editing"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#number-of-periods"));

        Assert.Null(component.Instance.EditingLessonPlan);
        Assert.Equal(createdTime, component.Instance.LessonPlan.CreatedDateTime);
        Assert.Equal(updatedTime, component.Instance.LessonPlan.UpdatedDateTime);
    }

    [Theory]
    [InlineData(1, 6)]
    [InlineData(2, 5)]
    [InlineData(4, 4)]
    [InlineData(5, 3)]
    [InlineData(7, 2)]
    [InlineData(8, 1)]
    public void AvailableLessonDurations_ShouldMatchWeekPlannerTemplate(int startPeriod, int expectedCount)
    {
        var appstate = CreateAppState();
        var component = RenderLessonPlanner(appstate, TestYear, FirstMonthOfSchool, FirstDayOfSchool, startPeriod);

        Assert.Equal(expectedCount, component.Instance.AvailableLessonSlots.Count);
        Assert.Equal([.. Enumerable.Range(1, expectedCount)], component.Instance.AvailableLessonSlots);
    }

    [Fact]
    public void ChangeLessonDuration_ShouldUpdateEditingLessonPlanAndDisplayCorrectValue()
    {
        var component = RenderLessonPlanner(_appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);
        component.Find("#edit-lesson-plan").Click();
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("3");
        var editingLessonPlan = component.Instance.EditingLessonPlan;
        Assert.NotNull(editingLessonPlan);
        Assert.Equal(3, editingLessonPlan.NumberOfPeriods);
        Assert.Equal("3", selectNumberOfPeriods.GetAttribute("value"));
    }

    [Fact]
    public void SubjectsDropdown_ShouldContainAllTaughtSubjectsAndNit()
    {
        var appState = CreateAppState();
        AddThreeSubjectsToYearData(appState.CurrentYearData);
        var component = RenderLessonPlanner(appState, 2025, 1, 29, 1);
        component.Find("#edit-lesson-plan").Click();
        var selectSubject = component.Find("select#subject-name");
        var options = selectSubject.GetElementsByTagName("option");

        var taughtSubjects = appState.CurrentYearData.SubjectsTaught;
        Assert.Equal(taughtSubjects.Count + 1, options.Length);
        foreach (var subject in taughtSubjects)
        {
            Assert.Contains(options, o => o.InnerHtml == subject.Name || o.InnerHtml == "NIT");
        }
    }

    [Fact]
    public void ChangeSelectedSubject_ShouldUpdateEditingLessonPlanAndDisplayCorrectValue()
    {
        var component = RenderLessonPlanner(_appState, 2025, 1, 29, 1);
        component.Find("#edit-lesson-plan").Click();
        var selectSubject = component.Find("select#subject-name");
        selectSubject.Change("Mathematics");
        var editingLessonPlan = component.Instance.EditingLessonPlan;
        Assert.NotNull(editingLessonPlan);
        Assert.Equal("Mathematics", editingLessonPlan.Subject.Name);
        Assert.Equal("Mathematics", selectSubject.GetAttribute("value"));
    }

    [Fact]
    public void SaveLessonPlan_ShouldPersistChangesAndExitEditMode()
    {
        var appState = CreateAppState();
        AddThreeSubjectsToYearData(appState.CurrentYearData);
        var component = RenderLessonPlanner(appState,  TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);

        component.Find("#edit-lesson-plan").Click();
        var selectSubject = component.Find("select#subject-name");
        selectSubject.Change("Mathematics");
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("2");
        component.Find("#save-lesson-plan").Click();

        Assert.False(component.Instance.IsInEditMode);
        Assert.NotNull(component.Find("p#subject-name"));
        Assert.NotNull(component.Find("p#number-of-periods"));
        Assert.NotNull(component.Find("#edit-lesson-plan"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("#save-lesson-plan"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("#cancel-editing"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#subject-name"));
        Assert.Throws<ElementNotFoundException>(() => component.Find("select#number-of-periods"));

        var lessonPlan = component.Instance.LessonPlan;
        Assert.Null(component.Instance.EditingLessonPlan);
        Assert.Equal("Mathematics", lessonPlan.Subject.Name);
        Assert.Equal(2, lessonPlan.NumberOfPeriods);
    }

    [Fact]
    public void SaveLessonPlan_WhenChangesOverlapLaterPlannedLesson_ShouldConfirmOverwrite()
    {
        //var appState = CreateAppState();
        //var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        //lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(It.IsAny<YearDataId>(), It.IsAny<DateOnly>(), 2, default))
        //   .ReturnsAsync(new LessonPlan(appState.CurrentYearData, new Subject([], "Science"), PeriodType.Lesson, "", 1, 2, new DateOnly(2025, 1, 29), []));
        //Services.AddScoped(sp => lessonPlanRepository.Object);
        //var component = RenderLessonPlanner(appState, 2025, 1, 29, 1);
        //component.Find("#edit-lesson-plan").Click();
        //var selectNumberOfPeriods = component.Find("select#number-of-periods");
        //selectNumberOfPeriods.Change("2");
        //component.Find("#save-lesson-plan").Click();
        //var overwriteDialog = component.FindComponent<Radzen.Dialog>();
        //Assert.NotNull(overwriteDialog);
        //Assert.Contains("The changes you made will overwrite an existing lesson plan.", overwriteDialog.Markup);
    }

    private IRenderedComponent<LessonPlanner> RenderLessonPlanner(AppState appState, int year, int month, int day, int startPeriod)
    {
        var component = RenderComponent<LessonPlanner>(parameters => parameters
            .Add(p => p.Year, year)
            .Add(p => p.Month, month)
            .Add(p => p.Day, day)
            .Add(p => p.StartPeriod, startPeriod)
            .Add(p => p.AppState, appState));

        return component;
    }

    private AppState CreateAppState()
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var subjectRepository = new Mock<ISubjectRepository>();
        var userRepository = new Mock<IUserRepository>();
        var weekPlannerRepository = new Mock<IWeekPlannerRepository>();
        var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        var yearDataRepository = new Mock<IYearDataRepository>();
        var logger = new Mock<ILogger<AppState>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var termDatesService = Helpers.CreateTermDatesService();
        var curriculumService = Helpers.CreateCurriculumService();

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        appState.CurrentYear = 2025;

        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(2025);

        var yearData = new YearData(Guid.NewGuid(), accountSetupState);
        var weekPlanner = new WeekPlanner(yearData, 2025, 1, 1, FirstDateOfSchool);
        var dayPlan = new DayPlan(weekPlanner.Id, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool), [], []);
        weekPlanner.UpdateDayPlan(dayPlan);
        yearData.AddWeekPlanner(weekPlanner);
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();

        appState.User = new User();
        var subject = new Subject("English", [], "");
        lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(yearData.Id, new DateOnly(2025, 1, 29), 1, default))
           .ReturnsAsync(new LessonPlan(yearData, subject, PeriodType.Lesson, "", 1, 1, new DateOnly(2025, 1, 29),
           [new Resource(appState.User.Id, "Test", "Url", false, subject, [])]));

        subjectRepository.Setup(cr => cr.GetSubjectById(It.IsAny<SubjectId>(), It.IsAny<CancellationToken>())).ReturnsAsync(curriculumService.CurriculumSubjects.First(s => s.Name == "Mathematics"));

        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearDataByYear.Add(yearData.CalendarYear, yearData);

        Services.AddScoped(sp => termDatesService);
        Services.AddScoped(sp => subjectRepository.Object);
        Services.AddScoped(sp => curriculumService);
        Services.AddScoped(sp => weekPlannerRepository.Object);
        Services.AddScoped(sp => lessonPlanRepository.Object);
        Services.AddScoped(sp => userRepository.Object);
        Services.AddScoped(sp => yearDataRepository.Object);
        Services.AddScoped(sp => unitOfWork.Object);

        Services.AddRadzenComponents();

        appState.GetType().GetProperty(nameof(appState.IsInitialised))!.SetValue(appState, true);
        return appState;
    }

    private static void AddThreeSubjectsToYearData(YearData yearData)
    {
        var mathSubject = new Subject("Mathematics", [], "");
        var engSubject = new Subject("English", [], "");
        var sciSubject = new Subject("Science", [], "");
        yearData.SubjectsTaught.Add(mathSubject);
        yearData.SubjectsTaught.Add(engSubject);
        yearData.SubjectsTaught.Add(sciSubject);
    }
}
