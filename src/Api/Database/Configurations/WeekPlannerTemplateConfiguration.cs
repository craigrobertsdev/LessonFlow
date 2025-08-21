using System.Text.Json;
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

        builder.OwnsMany(ws => ws.DayTemplates, dtb =>
        {
            dtb.ToTable("DayTemplates");
            dtb.Property<Guid>("Id");
            dtb.HasKey("Id");
            dtb.WithOwner().HasForeignKey("WeekPlannerTemplateId");

            dtb.Property(d => d.Periods)
                .HasConversion(
                    p => Serialize(p),
                    p => ConvertFromDtos(p));
        });
    }

    private static string Serialize(IEnumerable<PeriodBase> periods)
    {
        var dtos = periods.Select(PeriodToDto).ToList();
        return JsonSerializer.Serialize(dtos);
    }

    private static List<PeriodBase> ConvertFromDtos(string dtos)
    {
        var periods = JsonSerializer.Deserialize<List<PeriodDto>>(dtos);
        if (periods is null)
        {
            throw new Exception($"Could not deserialise: {nameof(periods)}");
        }

        return periods.Select(DtoToPeriod).ToList();
    }

    private class PeriodDto
    {
        public PeriodType Type { get; init; }
        public int StartPeriod { get; init; }
        public int NumberOfPeriods { get; init; }
        public string? SubjectName { get; init; }
        public string? BreakDuty { get; init; }
    }

    private static PeriodDto PeriodToDto(PeriodBase period)
    {
        return period switch
        {
            LessonStructure lesson => new PeriodDto
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

    private static PeriodBase DtoToPeriod(PeriodDto periodDto)
    {
        return periodDto.Type switch
        {
            PeriodType.Lesson => new LessonStructure(periodDto.SubjectName!, periodDto.StartPeriod,
                periodDto.NumberOfPeriods),
            PeriodType.Break => new BreakPeriod(periodDto.BreakDuty, periodDto.StartPeriod, periodDto.NumberOfPeriods),
            PeriodType.Nit => new NitPeriod(periodDto.StartPeriod, periodDto.NumberOfPeriods),
            _ => throw new ArgumentException($"Unknown period type: {nameof(periodDto)}"),
        };
    }
}