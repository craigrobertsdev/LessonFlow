using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Security.Claims;
using static LessonFlow.IntegrationTests.Helpers;

namespace LessonFlow.IntegrationTests.UI.LessonPlannerTests;
public class LessonPlannerTests : TestContext, IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AppState> _logger;

    public LessonPlannerTests(CustomWebApplicationFactory factory)
    {
        var scope = factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _factory = factory;

        _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<AppState>>();

        Services.AddSingleton(_userRepository);
        Services.AddSingleton(_logger);
        Services.AddSingleton(scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>());
        Services.AddSingleton(scope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddSingleton(scope.ServiceProvider.GetRequiredService<IYearDataRepository>());
        Services.AddSingleton(scope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddSingleton(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        this.AddTestAuthorization()
            .SetAuthorized("test@test.com")
            .SetClaims(new Claim(ClaimTypes.Name, "test@test.com"));
    }

    [Fact]
    public async Task SaveLessonPlan_WhenNoPreExistingLessonPlan_ShouldPersistNewLessonPlan()
    {
        var appState = await CreateAppState();
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, 1);

        component.WaitForElement("#edit-lesson-plan").Click();
        var selectSubject = component.Find("select#subject-name");
        selectSubject.Change("Mathematics");
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("2");
        component.Find("#save-lesson-plan").Click();

        using var scope = _factory.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedLessonPlan = dbContext.LessonPlans
            .Include(lp => lp.Subject)
            .FirstOrDefault(lp => lp.Id == component.Instance.LessonPlan.Id);

        Assert.NotNull(savedLessonPlan);
        Assert.Equal("Mathematics", savedLessonPlan.Subject.Name);
        Assert.Equal(2, savedLessonPlan.NumberOfPeriods);
    }

    [Fact]
    public async Task SaveLessonPlan_WhenPreExistingLesson_ShouldUpdateExistingLessonPlan()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var yearData = db.YearData.First(yd => yd.CalendarYear == 2025);
        var subject = db.Subjects.First(s => s.Name == "English");

        var startPeriod = 3;

        var lessonPlan = new LessonPlan(yearData, subject, PeriodType.Lesson, "", 1, startPeriod, FirstDateOfSchool, []);
        yearData.WeekPlanners.First(wp => wp.WeekStart == FirstDateOfSchool).DayPlans.First(dp => dp.Date == FirstDateOfSchool).AddLessonPlan(lessonPlan);
        db.LessonPlans.Add(lessonPlan);
        db.SaveChanges();

        var lessonPlanId = lessonPlan.Id;

        var appState = await CreateAppState();
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, startPeriod);

        component.WaitForElement("#edit-lesson-plan").Click();
        var selectSubject = component.WaitForElement("select#subject-name");
        selectSubject.Change("Mathematics");
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("2");
        component.Find("#save-lesson-plan").Click();

        using var newScope = _factory.Services.CreateScope();
        var dbContext = newScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedLessonPlan = dbContext.LessonPlans
            .Include(lp => lp.Subject)
            .FirstOrDefault(lp => lp.Id == component.Instance.LessonPlan.Id);

        Assert.NotNull(savedLessonPlan);
        Assert.Equal(lessonPlanId, savedLessonPlan.Id);
        Assert.Equal("Mathematics", savedLessonPlan.Subject.Name);
        Assert.Equal(2, savedLessonPlan.NumberOfPeriods);
    }

    private IRenderedComponent<LessonPlanner> RenderLessonPlanner(AppState appState, int year, int month, int day, int startPeriod)
    {

        return RenderComponent<LessonPlanner>(parameters => parameters
            .Add(p => p.Year, year)
            .Add(p => p.Month, month)
            .Add(p => p.Day, day)
            .Add(p => p.StartPeriod, startPeriod)
            .AddCascadingValue(appState));
    }

    private async Task<AppState> CreateAppState()
    {
        var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
        var appState = new AppState(authStateProvider, _userRepository, _logger);
        await appState.InitialiseAsync();

        return appState;
    }

    public new void Dispose()
    {
        _dbContext?.Dispose();
        base.Dispose();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
