using LessonFlow.Database.Converters;
using LessonFlow.Domain.YearPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

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

        builder.HasIndex(wp => new { wp.YearPlanId, wp.WeekStart })
            .IsUnique();

        builder.Navigation(wp => wp.DayPlans).AutoInclude();
    }
}