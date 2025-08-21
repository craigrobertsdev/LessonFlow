using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class WeekPlannerConfiguration : IEntityTypeConfiguration<WeekPlanner>
{
    public void Configure(EntityTypeBuilder<WeekPlanner> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.WeekPlannerIdConverter());

        builder.HasMany(wp => wp.DayPlans)
            .WithOne()
            .HasForeignKey("WeekPlannerId");

        builder.HasOne(wp => wp.YearData)
            .WithMany(yd => yd.WeekPlanners);
    }
}