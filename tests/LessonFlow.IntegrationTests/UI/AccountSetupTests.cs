using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
using LessonFlow.Domain.Enums;
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
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

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
            .SetAuthorized(TestUserNoAccountEmail)
            .SetClaims(new Claim(ClaimTypes.Name, TestUserNoAccountEmail));

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IUserRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IYearPlanRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(_ => _factoryScope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>());
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
        SeedDbContext(_dbContext);
    }

    [Fact]
    public async Task CompleteAccountSetup_WhenDataValid_DataCorrectlySaved()
    {
        var user = _dbContext.Users
            .Include(u => u.AccountSetupState)
            .First(u => u.Email == TestUserNoAccountEmail);
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
            new LessonTemplate("Science", 4, 1),
            new NitTemplate(5, 2),
            new BreakTemplate("", 6, 1),
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

    [Fact]
    public async Task CompleteAccountSetup_WhenSubjectsTaughtSelected_ArePersistedToDatabase()
    {
        var user = _dbContext.Users
            .Include(u => u.AccountSetupState)
            .First(u => u.Email == TestUserNoAccountEmail);
        var accountSetupState = new AccountSetupState(user.Id);
        var weekPlannerTemplate = IntegrationTestHelpers.GenerateWeekPlannerTemplate(user.Id);
        var weekPlannerTemplateId = weekPlannerTemplate.Id;
        weekPlannerTemplate.DayTemplates[0].BeforeSchoolDuty = "Before";
        weekPlannerTemplate.DayTemplates[0].AfterSchoolDuty = "After";
        accountSetupState.WeekPlannerTemplate = weekPlannerTemplate;
        accountSetupState.SetSchoolName("Test");
        accountSetupState.SetCalendarYear(2025);
        List<string> subjectNames = ["Mathematics", "Science"];
        accountSetupState.SetSubjectsTaught(subjectNames);
        accountSetupState.UpdateStep(AccountSetupStep.Subjects, ChangeDirection.Forward);
        accountSetupState.UpdateStep(AccountSetupStep.Timing, ChangeDirection.Forward);
        accountSetupState.UpdateStep(AccountSetupStep.Schedule, ChangeDirection.Forward);
        user.AccountSetupState = accountSetupState;
        _dbContext.SaveChanges();

        var appState = Services.GetRequiredService<AppState>();
        await appState.InitialiseAsync();

        var component = RenderAccountSetup();
        component.WaitForElement("#complete-account-setup").Click();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var yearPlan = db.YearPlans
            .Where(yp => yp.UserId == user.Id)
            .Include(yp => yp.SubjectsTaught)
            .Include(yp => yp.WeekPlannerTemplate)
            .FirstOrDefault();

        Assert.NotNull(yearPlan);
        Assert.Equal(2, yearPlan.SubjectsTaught.Count);
        var subjectsTaught = yearPlan.SubjectsTaught.Select(s => s.Name).ToList();
        subjectsTaught.Sort();
        subjectNames.Sort();
        Assert.Equal(subjectNames, subjectsTaught);

        weekPlannerTemplate = yearPlan.WeekPlannerTemplate;
        weekPlannerTemplate.DayTemplates.Sort((d1, d2) => d1.DayOfWeek.CompareTo(d2.DayOfWeek));
        Assert.NotNull(weekPlannerTemplate);
        Assert.Equal(weekPlannerTemplateId, weekPlannerTemplate.Id);
        Assert.Equal(5, weekPlannerTemplate.DayTemplates.Count);
        Assert.Equal(8, weekPlannerTemplate.Periods.Count);
        Assert.Equal("Before", weekPlannerTemplate.DayTemplates[0].BeforeSchoolDuty);
        Assert.Equal("After", weekPlannerTemplate.DayTemplates[0].AfterSchoolDuty);
    }

    [Fact]
    public void GoToNextStep_WhenStateValid_PersistsToDatabase()
    {
        List<DayOfWeek> workingDays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday];
        var component = RenderAccountSetup();
        component.WaitForState(() => component.Instance.AppState.IsInitialised);
        component.Instance.AccountSetupState.SetSchoolName("Test School");
        component.Instance.AccountSetupState.SetCalendarYear(2025);
        component.Instance.AccountSetupState.SetSubjectsTaught(["Mathematics", "Science"]);
        component.Instance.AccountSetupState.YearLevelsTaught.Add(YearLevelValue.Year6);
        component.Instance.AccountSetupState.WorkingDays.Clear();
        component.Instance.AccountSetupState.WorkingDays.AddRange(workingDays);
        component.WaitForElement("#next-step").Click();

        var context = _factory.Services.CreateScope()
            .ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = context.Users
            .Where(u => u.Email == TestUserNoAccountEmail)
            .Include(u => u.AccountSetupState)
            .First();

        Assert.Equal(AccountSetupStep.Subjects, user.AccountSetupState!.CurrentStep);
        Assert.Equal("Test School", user.AccountSetupState.SchoolName);
        Assert.Equal(2025, user.AccountSetupState.CalendarYear);
        user.AccountSetupState.WorkingDays.Sort();
        Assert.Equal(workingDays, user.AccountSetupState.WorkingDays);
    }

    // There are 2 issues occurring during account setup.
    // 1. When the user completes account setup, they are not redirected to weekplanner
    // 2. No daytemplates are being saved to the database resulting in an exception when trying to initialise the weekplanner
    
    /*
     * Need to update completed steps and current step when saving changes to accountsetupstate
     * Remove errorText as a column in the database
     * 
     */

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
