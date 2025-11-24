using LessonFlow.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace LessonFlow.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
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
            var dbContextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            var dbContextFactoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<ApplicationDbContext>));
            if (dbContextDescriptor is not null) services.Remove(dbContextDescriptor);
            if (dbContextOptionsDescriptor is not null) services.Remove(dbContextOptionsDescriptor);
            if (dbContextFactoryDescriptor is not null) services.Remove(dbContextFactoryDescriptor);

            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
            });

            services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                    .CreateDbContext());

            services.AddSingleton<IAmbientDbContextAccessor<ApplicationDbContext>, AmbientDbContextAccessor<ApplicationDbContext>>();
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory<ApplicationDbContext>>();

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthScheme;
            });

            services.AddAuthentication(TestAuthHandler.AuthScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthScheme, options => { });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        var context = Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        GC.SuppressFinalize(this);
        await base.DisposeAsync();
    }
}
