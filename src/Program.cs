using Asp.Versioning;
//using LessonFlow.Api.Services.CurriculumParser.SACurriculum;
using LessonFlow.Components;
using LessonFlow.Components.Account;
using LessonFlow.DependencyInjection;
using LessonFlow.Services;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

#if DEBUG
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(o => o.DetailedErrors = true);
#else
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
#endif

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddRadzenComponents();

        builder.Services.AddScoped<AppState>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration);

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapAdditionalIdentityEndpoints();

        //app.MapPost("api/dev/services/parse-curriculum", async (ApplicationDbContext context) =>
        //{
        //    var parser = new SACurriculumParser();
        //    var subjects = await parser.ParseCurriculum();
        //    context.CurriculumSubjects.AddRange(subjects);
        //    await context.SaveChangesAsync();
        //    return Results.Ok();
        //});

        app.MapPost("api/dev/services/term-dates", SetTermDates.Endpoint);

        app.Run(); 
    }
}
