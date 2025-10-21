using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Database;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using static LessonFlow.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Respawn;

namespace LessonFlow.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithUsername("lessonflow_user")
        .Build();

    private Respawner? _respawner;

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

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            WithReseed = true
        });

        await _respawner.ResetAsync(conn);

        // Re-seed to the known baseline state for each test
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SeedDbContext(db);
    }
}
