using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class WeekPlannerTemplateConfiguration : IEntityTypeConfiguration<WeekPlannerTemplate>
{
    public void Configure(EntityTypeBuilder<WeekPlannerTemplate> builder)
    {
        builder.HasKey(dp => dp.Id);
        builder.Property(dp => dp.Id)
            .HasConversion(new WeekPlannerTemplateId.StronglyTypedIdEfValueConverter());

        // Add shadow property for the foreign key to AccountSetupState
        builder.Property<Guid?>("AccountSetupStateId");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(dp => dp.UserId);

        builder.OwnsMany(dp => dp.Periods, pb =>
        {
            pb.ToTable("TemplatePeriods");
            pb.Property<Guid>("Id");
            pb.HasKey("Id");
            pb.Property(p => p.Name)
                .HasMaxLength(50);

            pb.Property(p => p.PeriodType)
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<PeriodType>(v))
                .HasMaxLength(20);
        });

        builder.HasMany(ws => ws.DayTemplates)
            .WithOne();

        builder.Navigation(wp => wp.Periods).AutoInclude();
        builder.Navigation(wp => wp.DayTemplates).AutoInclude();
    }
}

public class DayTemplateConfiguration : IEntityTypeConfiguration<DayTemplate>
{
    public void Configure(EntityTypeBuilder<DayTemplate> builder)
    {
        builder.ToTable("DayTemplates");
        builder.Property<Guid>("Id");
        builder.HasKey("Id");

        builder.HasOne<WeekPlannerTemplate>()
            .WithMany(wp => wp.DayTemplates)
            .HasForeignKey("WeekPlannerTemplateId")
            .IsRequired();

        builder.HasMany(dt => dt.Periods)
            .WithOne()
            .HasForeignKey("DayTemplateId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(dt => dt.Periods)
            .WithOne();

        builder.Property(dt => dt.BeforeSchoolDuty)
            .HasMaxLength(50);

        builder.Property(dt => dt.AfterSchoolDuty)
            .HasMaxLength(50);

        builder.Navigation(dt => dt.Periods).AutoInclude();
    }
}

public class PeriodBaseConfiguration : IEntityTypeConfiguration<PeriodBase>
{
    public void Configure(EntityTypeBuilder<PeriodBase> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.Property<Guid>("Id");
        builder.HasKey("Id");

        builder.HasOne<DayTemplate>()
            .WithMany(wp => wp.Periods)
            .HasForeignKey("DayTemplateId");
    }
}

public class PeriodDto
{
    public PeriodType Type { get; init; }
    public int StartPeriod { get; init; }
    public int NumberOfPeriods { get; init; }
    public string? SubjectName { get; init; }
    public string? BreakDuty { get; init; }

    public static PeriodDto PeriodToDto(PeriodBase period)
    {
        return period switch
        {
            LessonPeriod lesson => new PeriodDto
            {
                Type = PeriodType.Lesson,
                StartPeriod = lesson.StartPeriod,
                NumberOfPeriods = lesson.NumberOfPeriods,
                SubjectName = lesson.SubjectName,
            },
            BreakPeriod breakPeriod => new PeriodDto
            {
                Type = PeriodType.Break,
                StartPeriod = breakPeriod.StartPeriod,
                NumberOfPeriods = breakPeriod.NumberOfPeriods,
                BreakDuty = breakPeriod.BreakDuty
            },
            NitPeriod nit => new PeriodDto
            {
                Type = PeriodType.Nit,
                StartPeriod = nit.StartPeriod,
                NumberOfPeriods = nit.NumberOfPeriods,
            },
            _ => throw new ArgumentException($"Unknown period type: {nameof(period)}"),
        };
    }

    public static PeriodBase DtoToPeriod(PeriodDto periodDto)
    {
        return periodDto.Type switch
        {
            PeriodType.Lesson => new LessonPeriod(periodDto.SubjectName!, periodDto.StartPeriod,
                periodDto.NumberOfPeriods),
            PeriodType.Break => new BreakPeriod(periodDto.BreakDuty, periodDto.StartPeriod, periodDto.NumberOfPeriods),
            PeriodType.Nit => new NitPeriod(periodDto.StartPeriod, periodDto.NumberOfPeriods),
            _ => throw new ArgumentException($"Unknown period type: {nameof(periodDto)}"),
        };
    }
}
