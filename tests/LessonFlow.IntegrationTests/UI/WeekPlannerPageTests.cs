using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
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
        Services.AddScoped(sp =>scope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IYearPlanRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<ITermDatesService>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IUserRepository>());
        Services.AddScoped(sp => scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        SeedDbContext(_dbContext);

        this.AddTestAuthorization()
            .SetAuthorized("test@test.com")
            .SetClaims(new Claim(ClaimTypes.Name, "test@test.com"));
    }

    [Fact]
    public async Task HandleSaveChanges_WhenNoExistingWeekPlanner_ShouldCreateAndPersistNewWeekPlannerWithChanges()
    {
        var appState = await CreateAppState();
        appState.CurrentTerm = 1;
        appState.CurrentWeek = 1;
        appState.CurrentYear = TestYear;
        var existingWeekPlanner = appState.CurrentYearPlan.GetWeekPlanner(FirstDateOfSchool);

        var component = RenderWeekPlanner(appState);
        component.Find("#edit-week-planner").Click();
        component.Find("#before-school-duty-2").Change(new ChangeEventArgs() { Value = "Yard duty" });
        await component.Find("#save-changes").ClickAsync(new());

        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var yearPlan = db.YearPlans.First(yp => yp.Id == appState.CurrentYearPlan.Id);

        Assert.Null(existingWeekPlanner);
        var weekPlannerFromYearPlan = yearPlan.GetWeekPlanner(FirstDateOfSchool);
        Assert.NotNull(weekPlannerFromYearPlan);
        Assert.Equal("Yard duty", weekPlannerFromYearPlan!.GetDayPlan(FirstDateOfSchool)!.BeforeSchoolDuty);

        var weekPlannerFromDb = db.WeekPlanners
            .Where(wp => wp.YearPlanId == appState.CurrentYearPlan.Id)
            .FirstOrDefault(wp => wp.WeekStart == weekPlannerFromYearPlan.WeekStart);

        Assert.NotNull(weekPlannerFromDb);
        Assert.Equal("Yard duty", weekPlannerFromDb!.GetDayPlan(FirstDateOfSchool)!.BeforeSchoolDuty);
    }

    public async Task HandleSaveChanges_WhenExistingLessonPlan_ShouldCreateAndPersistNewLessonPlanWithChanges()
    {
        throw new NotImplementedException();
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
