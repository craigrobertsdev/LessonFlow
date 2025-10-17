using Bunit;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared;
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
        _appState = CreateAppState(2025);
        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
    }

    [Fact]
    public void OnInitialized_WhenCalledWithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var year = 2025;
        var month = 1;
        var day = 29;
        var periodStart = 1;
        var component = RenderLessonPlanner(_appState, year, month, day, periodStart);

        Assert.NotNull(component.Instance.LessonPlan);
        Assert.Equal(new DateOnly(year, month, day), component.Instance.LessonPlan.LessonDate);
        Assert.Equal(periodStart, component.Instance.LessonPlan.StartPeriod);
    }

    [Fact]
    public void OnInitialised_WhenNoLessonPlanned_ShouldCreateNewLessonPlan()
    {
        // Arrange
        var year = 2025;
        var month = 1;
        var day = 30; // No lesson planned on this date in the mock setup
        var periodStart = 1;
        var component = RenderLessonPlanner(_appState, year, month, day, periodStart);
        // Act
        var lessonPlan = component.Instance.LessonPlan;
        // Assert
        Assert.NotNull(lessonPlan);
        Assert.Equal(new DateOnly(year, month, day), lessonPlan.LessonDate);
        Assert.Equal(periodStart, lessonPlan.StartPeriod);
    }

    [Theory]
    [InlineData(1, 1, "Mathematics")]
    [InlineData(2, 2, "English")]
    public void OnInitialised_WhenNoLessonPlanned_ShouldCreateLessonPlanFromWeekPlannerTemplate(int periodStart, int numberOfPeriods, string subjectName)
    {

        // Arrange
        var year = 2025;
        var month = 1;
        var day = 30;
        var date = new DateOnly(year, month, day);
        var appState = CreateAppState(year);
        var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(It.IsAny<YearDataId>(), It.IsAny<DateOnly>(), periodStart, default))
           .ReturnsAsync(new LessonPlan(appState.CurrentYearData, new Subject([], subjectName), PeriodType.Lesson, "", numberOfPeriods, periodStart, date, []));

        Services.AddScoped(sp => lessonPlanRepository.Object);
        appState.CurrentYearData.WeekPlannerTemplate.DayTemplates[1].Periods[0] = new LessonPeriod(subjectName, periodStart, numberOfPeriods);
        var component = RenderLessonPlanner(appState, year, month, day, periodStart);
        // Act
        var lessonPlan = component.Instance.LessonPlan;
        // Assert
        Assert.NotNull(lessonPlan);
        Assert.Equal(date, lessonPlan.LessonDate);
        Assert.Equal(periodStart, lessonPlan.StartPeriod);
        Assert.Equal(subjectName, lessonPlan.Subject.Name);
        Assert.Equal(numberOfPeriods, lessonPlan.NumberOfPeriods);
    }

    private IRenderedComponent<LessonPlanner> RenderLessonPlanner(AppState appState, int year, int month, int day, int periodStart)
    {
        var component = RenderComponent<LessonPlanner>(parameters => parameters
            .Add(p => p.Year, year)
            .Add(p => p.Month, month)
            .Add(p => p.Day, day)
            .Add(p => p.PeriodStart, periodStart)
            .Add(p => p.AppState, appState));

        return component;
    }

    private AppState CreateAppState(int calendarYear)
    {
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        var userRepository = new Mock<IUserRepository>();
        var weekPlannerRepository = new Mock<IWeekPlannerRepository>();
        var lessonPlanRepository = new Mock<ILessonPlanRepository>();
        var yearDataRepository = new Mock<IYearDataRepository>();
        var logger = new Mock<ILogger<AppState>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var termDatesService = Helpers.CreateTermDatesService();

        var appState = new AppState(authStateProvider.Object, userRepository.Object, logger.Object);
        appState.CurrentYear = calendarYear;

        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(calendarYear);


        var yearData = new YearData(Guid.NewGuid(), accountSetupState);
        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate();

        lessonPlanRepository.Setup(r => r.GetByDateAndPeriodStart(yearData.Id, new DateOnly(2025, 1, 29), 1, default))
           .ReturnsAsync(new LessonPlan(yearData, new Subject("English", [], ""), PeriodType.Lesson, "", 1, 1, new DateOnly(2025, 1, 29), []));
        yearData.WeekPlannerTemplate = weekPlannerTemplate;
        appState.YearDataByYear.Add(yearData.CalendarYear, yearData);
        appState.User = new User();

        Services.AddScoped(sp => termDatesService);
        Services.AddScoped(sp => weekPlannerRepository.Object);
        Services.AddScoped(sp => lessonPlanRepository.Object);
        Services.AddScoped(sp => userRepository.Object);
        Services.AddScoped(sp => yearDataRepository.Object);
        Services.AddScoped(sp => unitOfWork.Object);

        Services.AddRadzenComponents();

        appState.GetType().GetProperty(nameof(appState.IsInitialised))!.SetValue(appState, true);
        return appState;
    }

}
