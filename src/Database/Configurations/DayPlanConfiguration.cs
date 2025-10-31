using System.Text.Json;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.YearPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class DayPlanConfiguration : IEntityTypeConfiguration<DayPlan>
{
    public void Configure(EntityTypeBuilder<DayPlan> builder)
    {
        builder.HasKey(dp => dp.Id);
        builder.Property(dp => dp.Id)
            .HasColumnName("Id")
            .HasConversion(new DayPlanId.StronglyTypedIdEfValueConverter());

        builder.HasMany(dp => dp.LessonPlans)
            .WithOne()
            .HasForeignKey(lp => lp.DayPlanId)
            .IsRequired();

        builder.HasMany(dp => dp.SchoolEvents)
            .WithMany();

        builder.Property(dp => dp.BreakDutyOverrides)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                v => JsonSerializer.Deserialize<Dictionary<int, string>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<int, string>()
                );

        builder.Property(dp => dp.BeforeSchoolDuty)
            .HasMaxLength(50);

        builder.Property(dp => dp.AfterSchoolDuty)
            .HasMaxLength(50);
    }
}