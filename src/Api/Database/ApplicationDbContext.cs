using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.Calendar;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Reports;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Api.Database;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly IPublisher _publisher = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Subject> CurriculumSubjects { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Assessment> Assessments { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<LessonPlan> LessonPlans { get; set; } = null!;
    public DbSet<WeekPlanner> WeekPlanners { get; set; } = null!;
    public DbSet<WeekPlannerTemplate> WeekPlannerTemplates { get; set; } = null!;
    public DbSet<TermPlanner> TermPlanners { get; set; } = null!;
    public DbSet<Calendar> Calendar { get; set; } = null!;
    public DbSet<YearData> YearData { get; set; } = null!;
    public DbSet<SchoolTerm> TermDates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<UserId>();

        modelBuilder
            .Ignore<List<IDomainEvent>>()
            .ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(UserId))
                {
                    property.SetValueConverter(new UserId.StronglyTypedIdEfValueConverter());
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var entitiesWithDomainEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithDomainEvents.ForEach(e => e.ClearDomainEvents());

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}