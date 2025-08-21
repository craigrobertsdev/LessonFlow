using System.Text.Json;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.WeekPlanners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

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
            .HasForeignKey("DayPlanId")
            .IsRequired();

        builder.HasMany(dp => dp.SchoolEvents)
            .WithMany();

        builder.Property<Dictionary<int, string>?>("_breakDutyOverrides")
            .HasColumnName("BreakDutyOverrides")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                v => JsonSerializer.Deserialize<Dictionary<int, string>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                );
    }
}