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
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

namespace LessonFlow.IntegrationTests.UI;

[CollectionDefinition("Non-ParallelTests", DisableParallelization = true)]
[Collection("Non-ParallelTests")]
public class LessonPlannerTests : TestContext, IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;

    public LessonPlannerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<IYearDataRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();
        SeedDbContext(_dbContext);

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
        var selectSubject = component.WaitForElement("select#subject-name");
        selectSubject.Change("Mathematics");
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("2");
        component.Find("#save-lesson-plan").Click();

        var savedLessonPlan = _dbContext.LessonPlans
            .Include(lp => lp.Subject)
            .FirstOrDefault(lp => lp.Id == component.Instance.LessonPlan.Id);

        Assert.NotNull(savedLessonPlan);
        Assert.Equal("Mathematics", savedLessonPlan.Subject.Name);
        Assert.Equal(2, savedLessonPlan.NumberOfPeriods);
    }

    [Fact]
    public async Task SaveLessonPlan_WhenPreExistingLesson_ShouldUpdateExistingLessonPlan()
    {
        var user = _dbContext.Users.First(u => u.Email == "test@test.com");
        var dayPlan = _dbContext.Users.First(u => u.Email == "test@test.com")
            .YearDataHistory.First(yd => yd.CalendarYear == TestYear)
                .WeekPlanners.First().DayPlans.First();
        var subject = _dbContext.Subjects.First(s => s.Name == "English");

        var startPeriod = 3;

        var lessonPlan = new LessonPlan(dayPlan.Id, subject, PeriodType.Lesson, "", 1, startPeriod, FirstDateOfSchool, []);
        dayPlan.AddLessonPlan(lessonPlan);
        _dbContext.LessonPlans.Add(lessonPlan);
        _dbContext.SaveChanges();

        var lessonPlanId = lessonPlan.Id;

        var appState = await CreateAppState();
        appState.CurrentYearData.WeekPlanners.First().DayPlans[0] = dayPlan;
        var component = RenderLessonPlanner(appState, TestYear, FirstMonthOfSchool, FirstDayOfSchool, startPeriod);

        component.WaitForElement("#edit-lesson-plan").Click();
        var selectSubject = component.WaitForElement("select#subject-name");
        selectSubject.Change("Mathematics");
        var selectNumberOfPeriods = component.Find("select#number-of-periods");
        selectNumberOfPeriods.Change("2");
        component.Find("#save-lesson-plan").Click();

        var savedLessonPlan = _dbContext.LessonPlans
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
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppState>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
        var appState = new AppState(authStateProvider, userRepository, logger);
        await appState.InitialiseAsync();

        return appState;
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        _dbContext?.Dispose();
        _scope?.Dispose();
        base.Dispose();
    }
}
