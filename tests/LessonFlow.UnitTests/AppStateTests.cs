using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using static LessonFlow.UnitTests.UnitTestHelpers;
using Moq;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Domain.Users;
using LessonFlow.Shared.Interfaces.Services;
using System.Security.Claims;

namespace LessonFlow.UnitTests;
public class AppStateTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task InitialiseAsync_WhenCalled_ShouldSetTermNumberCorrectly(int termNumber)
    {
        var appState = CreateAppState(termNumber, 1);

        await appState.InitialiseAsync();

        Assert.Equal(termNumber, appState.CurrentTerm);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(11)]
    public async Task InitialiseAsync_WhenCalled_ShouldSetWeekNumberCorrectly(int weekNumber)
    {
        var appState = CreateAppState(1, weekNumber);

        await appState.InitialiseAsync();
        
        Assert.Equal(weekNumber, appState.CurrentWeek);
    }

    private AppState CreateAppState(int termNumber, int weekNumber)
    {
        var authStateProvider = new CustomAuthenticationStateProvider();
        var logger = new Mock<ILogger<AppState>>();
        var userRepository = new Mock<IUserRepository>();
        var termDatesService = new Mock<ITermDatesService>();

        var appState = new AppState(authStateProvider, userRepository.Object, logger.Object, termDatesService.Object)
        {
            CurrentYear = TestYear
        };

        var user = new User()
        {
            Email = "test@test.com",
            UserName = "test@test.com"
        };
        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(TestYear);
        user.AccountSetupState = accountSetupState;
        user.CompleteAccountSetup();
        userRepository.Setup(ur => ur.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userRepository.Setup(ur => ur.GetYearPlanByYear(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var yearPlan = new YearPlan(Guid.NewGuid(), accountSetupState, []);
                return yearPlan;
            });


        termDatesService.Setup(tds => tds.GetTermNumber(It.IsAny<DateOnly>())).Returns(termNumber);
        termDatesService.Setup(tds => tds.GetWeekNumber(It.IsAny<DateOnly>())).Returns(weekNumber);

        return appState;
    }

    class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test@test.com"),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }


    }
}
