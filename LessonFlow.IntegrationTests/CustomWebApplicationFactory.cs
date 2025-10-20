using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Database;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace LessonFlow.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithUsername("lessonflow_user")
        .Build();

    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>

        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var csb = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
                {
                    IncludeErrorDetail = true
                };

                options.UseNpgsql(csb.ConnectionString);
            });

            var dbContext = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            SeedDbContext(dbContext);

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthScheme;
            });

            services.AddAuthentication(TestAuthHandler.AuthScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthScheme, options => { });

        });
    }

    private void SeedDbContext(ApplicationDbContext dbContext)
    {
        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(2025);

        var user = new User
        {
            AccountSetupState = accountSetupState,
            Email = "test@test.com",
            UserName = "testuser",
            LastSelectedYear = 2025
        };
        user.CompleteAccountSetup();
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        user = dbContext.Users.First();

        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate(user.Id);
        var yearData = new YearData(user.Id, weekPlannerTemplate, "Test School", 2025);
        var weekPlanner = new WeekPlanner(yearData, 2025, 1, 1, new DateOnly(2025, 1, 27));
        var dayPlan = new DayPlan(weekPlanner.Id, new DateOnly(2025, 1, 29), [], []);
        weekPlanner.UpdateDayPlan(dayPlan);
        yearData.AddWeekPlanner(weekPlanner);
        dbContext.YearData.Add(yearData);
        dbContext.SaveChanges();

        user.AddYearData(yearData);
        dbContext.SaveChanges();

        var subjects = new List<Subject>
        {
            new([], "Mathematics"),
            new([], "Science"),
            new([], "English"),
        };

        dbContext.Subjects.AddRange(subjects);
        dbContext.SaveChanges();

        yearData.AddSubjects(subjects);
        dbContext.SaveChanges();
    }
}
