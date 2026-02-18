using System.Security.Claims;
using Bunit;
using LessonFlow.Database;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using TestComponents;
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

namespace LessonFlow.IntegrationTests;

[Collection("Non-ParallelTests")]
public class AppStateTests : BunitContext, IClassFixture<CustomWebApplicationFactory>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;

    public AppStateTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ICurriculumService>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<IYearPlanRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ISubjectRepository>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>());
        Services.AddScoped(sp => _scope.ServiceProvider.GetRequiredService<ITermDatesService>());
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();

        JSInterop.SetupVoid("Radzen.createEditor", _ => true);
        JSInterop.SetupVoid("Radzen.innerHTML", _ => true);

        AddAuthorization()
            .SetAuthorized("test@test.com")
            .SetClaims(new Claim(ClaimTypes.Name, "test@test.com"));

        SeedDbContext(_dbContext);
    }

    [Fact]
    public void InitialiseAsync_WhenLessonsTaughtOnYearPlan_ShouldRetrieveFromDatabase()
    {
        var appState = new AppState(
            Services.GetRequiredService<AuthenticationStateProvider>(),
            _scope.ServiceProvider.GetRequiredService<IUserRepository>(),
            _scope.ServiceProvider.GetRequiredService<ILogger<AppState>>(),
            _scope.ServiceProvider.GetRequiredService<ITermDatesService>());

        var component = Render<AppStateTestComponent>(p => p.Add(c => c.AppState, appState));

        component.WaitForState(() => appState.IsInitialised);

        var yearPlan = appState.CurrentYearPlan;
        Assert.NotNull(yearPlan);
        Assert.NotEmpty(yearPlan.SubjectsTaught);
    }
}