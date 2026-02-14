using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.Calendar;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Reports;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Services.FileStorage;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly IPublisher _publisher = null!;

    //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
    //    : base(options)
    //{
    //    _publisher = publisher;
    //}

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Subject> Subjects { get; set; } = null!;
    public virtual DbSet<Resource> Resources { get; set; } = null!;
    public virtual DbSet<Student> Students { get; set; } = null!;
    public virtual DbSet<Assessment> Assessments { get; set; } = null!;
    public virtual DbSet<Report> Reports { get; set; } = null!;
    public virtual DbSet<LessonPlan> LessonPlans { get; set; } = null!;
    public virtual DbSet<WeekPlanner> WeekPlanners { get; set; } = null!;
    public virtual DbSet<WeekPlannerTemplate> WeekPlannerTemplates { get; set; } = null!;
    public virtual DbSet<TermPlanner> TermPlanners { get; set; } = null!;
    public virtual DbSet<Calendar> Calendar { get; set; } = null!;
    public virtual DbSet<YearPlan> YearPlans { get; set; } = null!;
    public virtual DbSet<SchoolTerm> TermDates { get; set; } = null!;
    public virtual DbSet<FileSystem> FileSystems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<UserId>();

        modelBuilder.Entity<LessonTemplate>().HasBaseType<PeriodTemplateBase>();
        modelBuilder.Entity<BreakTemplate>().HasBaseType<PeriodTemplateBase>();
        modelBuilder.Entity<NitTemplate>().HasBaseType<PeriodTemplateBase>();

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

    public override async Task<int> SaveChangesAsync(CancellationToken ct = new())
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
            var result = await base.SaveChangesAsync(ct);

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, ct);
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