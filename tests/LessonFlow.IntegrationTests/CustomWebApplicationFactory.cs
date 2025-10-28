using LessonFlow.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LessonFlow.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddJsonFile("appsettings.test.json");
        });

        builder.ConfigureServices(services =>
        {
            string connectionString;

            using (var tempScope = services.BuildServiceProvider().CreateScope())
            {
                var configuration = tempScope.ServiceProvider.GetRequiredService<IConfiguration>();
                connectionString = configuration.GetConnectionString("TestConnection")!;
            }

            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }
            if (dbContextOptionsDescriptor is not null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
            });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthScheme;
            });

            services.AddAuthentication(TestAuthHandler.AuthScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthScheme, options => { });
        });
    }
}
