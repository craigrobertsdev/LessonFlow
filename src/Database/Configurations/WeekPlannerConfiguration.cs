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

        builder.Ignore(w => w.HasLessonPlansLoaded);

        builder.HasMany(wp => wp.DayPlans)
            .WithOne()
            .HasForeignKey("WeekPlannerId");

        builder.HasIndex(wp => new { wp.YearPlanId, wp.WeekStart })
            .IsUnique();

        builder.Navigation(wp => wp.DayPlans).AutoInclude();

        builder.OwnsMany(wp => wp.Todos, wptd =>
        {
            wptd.ToTable("TodoItem");
            wptd.WithOwner().HasForeignKey(td => td.WeekPlannerId);
            wptd.Property<Guid>("Id");
            wptd.HasKey("Id", "WeekPlannerId");
            wptd.Ignore(td => td.IsMouseOver);
        });
    }
}