using System.Text.Json;
using LessonFlow.Shared.Interfaces.Services;
using LessonFlow.Components.Account;
using LessonFlow.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LessonFlow.Database;
using LessonFlow.Database.Repositories;
using LessonFlow.Services;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Persistence;
//using LessonFlow.Services.CurriculumParser.SACurriculum;


namespace LessonFlow.DependencyInjection;

public static class Infrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddServices();
        services.AddAuth();
        services.AddCurriculumParser();
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dbContextSettings = new DbContextSettings();
#if DEBUG
        configuration.Bind(DbContextSettings.SectionName, dbContextSettings);
#else
        var dbConfig = SecretManager.GetDbConfig();
        dbContextSettings.DefaultConnection =
 $"server=db;port={dbConfig.Port};user={dbConfig.User};password={dbConfig.Password};database={dbConfig.DbName}";

#endif

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(dbContextSettings.DefaultConnection)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors());

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        
        services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ILessonPlanRepository, LessonPlanRepository>();
        services.AddScoped<ITermPlannerRepository, TermPlannerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWeekPlannerRepository, WeekPlannerRepository>();
        services.AddScoped<IYearDataRepository, YearDataRepository>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurriculumService, CurriculumService>();
        services.AddSingleton<ITermDatesService, TermDatesService>();
        services.AddTransient<IStorageManager, StorageManager>();
        //services.AddScoped<ICurriculumParser, SACurriculumParser>();

        return services;
    }

    private static void AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();
    }

    /// <summary>
    ///     Used to manage retrieving secrets when published in a Docker Container.
    /// </summary>
    private class SecretManager
    {
        public static DbConfig GetDbConfig()
        {
            var text = File.ReadAllText("/run/secrets/DB_CONFIG");
            return JsonSerializer.Deserialize<DbConfig>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }

    private static IServiceCollection AddCurriculumParser(this IServiceCollection services)
    {
        //services.AddScoped<ICurriculumParser, SACurriculumParser>();
        return services;
    }

    public static void ConfigureIdentity(IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options => { options.Password.RequiredLength = 14; });
    }

    private class DbConfig
    {
        public string Port { get; set; } = null!;
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string DbName { get; set; } = null!;
    }
}