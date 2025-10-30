using Bunit;
using Bunit.TestDoubles;
using LessonFlow.Components.Pages;
using LessonFlow.Database;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Security.Claims;

namespace LessonFlow.IntegrationTests.UI;
public class WeekPlannerPageTests : TestContext, IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AppState> _logger;
    public WeekPlannerPageTests(CustomWebApplicationFactory factory)
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

    //[Fact]
    //public async Task 

    private IRenderedComponent<WeekPlannerPage> RenderLessonPlanner(AppState appState)
    {
        return RenderComponent<WeekPlannerPage>(parameters => parameters
        .Add(w => w.AppState, appState));
    }

    private async Task<AppState> CreateAppState()
    {
        var authStateProvider = Services.GetRequiredService<AuthenticationStateProvider>();
        var termDatesService = Services.GetRequiredService<ITermDatesService>();

        var appState = new AppState(authStateProvider, _userRepository, _logger, termDatesService);
        await appState.InitialiseAsync();

        return appState;
    }
}
