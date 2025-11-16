using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Security.Claims;
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

namespace LessonFlow.IntegrationTests.UI;

[Collection("Non-ParallelTests")]
public class WeekPlannerPageTests : TestContext, IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory _factory;

    public WeekPlannerPageTests(CustomWebApplicationFactory factory)
    {
        var scope = factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _factory = factory;

        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IYearPlanRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ITermDatesService>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IUserRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>());
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        SeedDbContext(_dbContext);

        this.AddTestAuthorization()
            .SetAuthorized(TestUserEmail)
            .SetClaims(new Claim(ClaimTypes.Name, TestUserEmail));
    }

    [Fact]
    public async Task Initialise_WhenWeekPlannerExistsButNoDayPlansLoaded_LoadDayPlans()
    {
        var dbDayPlan = _dbContext.WeekPlanners.First().DayPlans.First();
        var subject = _dbContext.Subjects.First();
        var lessonPlan = new LessonPlan(dbDayPlan.Id, subject, PeriodType.Lesson, string.Empty, 1, 1, FirstDateOfSchool, []);
        dbDayPlan.AddLessonPlan(lessonPlan);
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        var yearPlan = _dbContext.YearPlans.First(yp => yp.CalendarYear == TestYear && yp.UserId == user.Id);
        var weekPlanner = _dbContext.WeekPlanners.First();
        weekPlanner.UpdateDayPlan(dbDayPlan);
        yearPlan.AddWeekPlanner(weekPlanner);
        _dbContext.SaveChanges();

        var appState = await CreateAppState();
        appState.CurrentWeek = 1;
        appState.CurrentTerm = 1;
        appState.CurrentYear = TestYear;

        //var dayPlans = appState.CurrentYearPlan.GetWeekPlanner(FirstDateOfSchool)!.DayPlans;
        //var initialLessonPlans = dayPlans[0].LessonPlans;
        var component = RenderWeekPlanner(appState);

        //Assert.Empty(initialLessonPlans);
        component.WaitForState(() => component.Instance.GridCols.Count == 5);
        var appStateWeekPlanner = component.Instance.WeekPlanner;
        Assert.NotNull(appStateWeekPlanner);

        var componentLessonPlan = appStateWeekPlanner.DayPlans[0].LessonPlans.First();
        Assert.Single(appStateWeekPlanner.DayPlans[0].LessonPlans);
        Assert.Equal(lessonPlan.Id, componentLessonPlan.Id);
        Assert.Equal(lessonPlan.Subject.Name, componentLessonPlan.Subject.Name);
    }

    [Fact]
    public async Task HandleSaveChanges_WhenNoExistingWeekPlanner_ShouldCreateAndPersistNewWeekPlannerWithChanges()
    {
        var appState = await CreateAppState();
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 2;
        appState.CurrentYear = TestYear;
        var testDate = FirstDateOfSchool.AddDays(7);
        var existingWeekPlanner = appState.CurrentYearPlan.GetWeekPlanner(testDate);

        var component = RenderWeekPlanner(appState);
        component.WaitForElement("#edit-week-planner").Click();
        component.Find("#before-school-duty-2").Change(new ChangeEventArgs() { Value = "Yard duty" });
        await component.Find("#save-changes").ClickAsync(new());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var yearPlan = db.YearPlans.First(yp => yp.Id == appState.CurrentYearPlan.Id);

        Assert.Null(existingWeekPlanner);
        var weekPlannerFromYearPlan = yearPlan.GetWeekPlanner(testDate);
        Assert.NotNull(weekPlannerFromYearPlan);
        Assert.Equal("Yard duty", weekPlannerFromYearPlan!.GetDayPlan(testDate)!.BeforeSchoolDuty);

        var weekPlannerFromDb = db.WeekPlanners
            .Where(wp => wp.YearPlanId == appState.CurrentYearPlan.Id)
            .FirstOrDefault(wp => wp.WeekStart == weekPlannerFromYearPlan.WeekStart);

        Assert.NotNull(weekPlannerFromDb);
        Assert.Equal("Yard duty", weekPlannerFromDb!.GetDayPlan(testDate)!.BeforeSchoolDuty);
    }

    [Fact]
    public async Task HandleSaveChanges_WhenExistingWeekPlanner_ShouldUpdate()
    {
        var appState = await CreateAppState();
        appState.CurrentYear = TestYear;
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;

        var component = RenderWeekPlanner(appState);

        component.WaitForElement("#edit-week-planner").Click();
        component.WaitForElement("#before-school-duty-2").Change(new ChangeEventArgs() { Value = "Yard duty" });
        await component.WaitForElement("#save-changes").ClickAsync(new());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var yearPlan = db.YearPlans.First(yp => yp.Id == appState.CurrentYearPlan.Id);

        var weekPlannerFromYearPlan = yearPlan.GetWeekPlanner(FirstDateOfSchool);
        Assert.NotNull(weekPlannerFromYearPlan);
        Assert.Equal("Yard duty", weekPlannerFromYearPlan!.GetDayPlan(FirstDateOfSchool)!.BeforeSchoolDuty);

        var weekPlannerFromDb = db.WeekPlanners
            .Where(wp => wp.YearPlanId == appState.CurrentYearPlan.Id)
            .FirstOrDefault(wp => wp.WeekStart == weekPlannerFromYearPlan.WeekStart);

        Assert.NotNull(weekPlannerFromDb);
        Assert.Equal("Yard duty", weekPlannerFromDb!.GetDayPlan(FirstDateOfSchool)!.BeforeSchoolDuty);
    }

    private IRenderedComponent<WeekPlannerPage> RenderWeekPlanner(AppState appState)
    {
        return RenderComponent<WeekPlannerPage>(parameters => parameters
        .Add(w => w.AppState, appState));
    }

    private async Task<AppState> CreateAppState()
    {
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppState>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
        var termDatesService = Services.GetRequiredService<ITermDatesService>();
        var appState = new AppState(authStateProvider, userRepository, logger, termDatesService);
        await appState.InitialiseAsync();

        return appState;
    }
}
