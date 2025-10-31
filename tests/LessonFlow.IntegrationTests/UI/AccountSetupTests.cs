using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Security.Claims;

namespace LessonFlow.IntegrationTests.UI;

[Collection("Non-ParallelTests")]
public class AccountSetupTests : TestContext, IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _factoryScope;
    private readonly ApplicationDbContext _dbContext;

    public AccountSetupTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factoryScope = factory.Services.CreateScope();

        this.AddTestAuthorization()
            .SetAuthorized("accountsetupnotcomplete@test.com")
            .SetClaims(new Claim(ClaimTypes.Name, "accountsetupnotcomplete@test.com"));

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        Services.AddSingleton(_ => _factoryScope.ServiceProvider.GetRequiredService<ILoggerFactory>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IUserRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IYearPlanRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ITermDatesService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<AppState>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<DialogService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<NotificationService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<TooltipService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ContextMenuService>());


        Services.AddScoped(sp =>
        {
            var auth = sp.GetRequiredService<AuthenticationStateProvider>();
            var repo = sp.GetRequiredService<IUserRepository>();
            var logger = sp.GetRequiredService<ILogger<AppState>>();
            var termDatesService = sp.GetRequiredService<ITermDatesService>();

            return new AppState(auth, repo, logger, termDatesService);
        });

        _dbContext = _factoryScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task AccountSetup_WhenUserCompletesProcess_DataCorrectlySaved()
    {
        var user = _dbContext.Users
            .Include(u => u.AccountSetupState)
            .First(u => u.Email == "accountsetupnotcomplete@test.com");
        var accountSetupState = new AccountSetupState(user.Id);
        var weekPlannerTemplate = IntegrationTestHelpers.GenerateWeekPlannerTemplate(user.Id);
        var weekPlannerTemplateId = weekPlannerTemplate.Id;
        accountSetupState.WeekPlannerTemplate = weekPlannerTemplate;
        accountSetupState.SetSchoolName("Test");
        accountSetupState.SetCalendarYear(2025);
        accountSetupState.SetSubjectsTaught(["Mathematics", "Science"]);
        accountSetupState.WeekPlannerTemplate.DayTemplates[0].Periods.Clear();
        accountSetupState.WeekPlannerTemplate.DayTemplates[0].Periods.AddRange([
            new LessonTemplate("Mathematics", 1, 2),
            new BreakTemplate("", 3, 1),
            new LessonTemplate("Science", 3, 1),
            new NitTemplate(4, 2),
            new BreakTemplate("", 5, 1),
            new LessonTemplate("Mathematics", 7, 2)
        ]);
        accountSetupState.GetType().GetProperty("CurrentStep")!.SetValue(accountSetupState, AccountSetupStep.Schedule);

        user.AccountSetupState = accountSetupState;
        _dbContext.Users.Update(user);
        _dbContext.SaveChanges();

        var appState = Services.GetRequiredService<AppState>();
        await appState.InitialiseAsync();

        var component = RenderAccountSetup();
        component.WaitForElement("#complete-account-setup").Click();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        weekPlannerTemplate = db.WeekPlannerTemplates.First(wp => wp.Id == weekPlannerTemplateId);

        var dayTemplate = weekPlannerTemplate.DayTemplates[0];
        dayTemplate.Periods.Sort((a, b) => a.StartPeriod.CompareTo(b.StartPeriod));

        Assert.Equal(6, dayTemplate.Periods.Count);
        Assert.Equal(typeof(LessonTemplate), dayTemplate.Periods[0].GetType());
        Assert.Equal(typeof(BreakTemplate), dayTemplate.Periods[1].GetType());
        Assert.Equal(typeof(LessonTemplate), dayTemplate.Periods[2].GetType());
        Assert.Equal(typeof(NitTemplate), dayTemplate.Periods[3].GetType());
        Assert.Equal(typeof(BreakTemplate), dayTemplate.Periods[4].GetType());
        Assert.Equal(typeof(LessonTemplate), dayTemplate.Periods[5].GetType());
    }

    private IRenderedComponent<AccountSetup> RenderAccountSetup()
    {
        var accountSetupComponent = RenderComponent<AccountSetup>();

        return accountSetupComponent;
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}
