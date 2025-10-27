using LessonFlow.Database.Converters;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.WeekPlanners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.HasKey(lp => lp.Id);

        builder.Property(lp => lp.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.LessonPlanIdConverter());

        builder.HasOne<DayPlan>()
            .WithMany(yd => yd.LessonPlans)
            .HasForeignKey(lp => lp.DayPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lp => lp.Subject)
            .WithMany();

        builder.HasMany(lp => lp.Resources)
            .WithMany(r => r.LessonPlans);

        builder.OwnsMany(lp => lp.Comments, lcb =>
        {
            lcb.ToTable("LessonComment");
            lcb.WithOwner().HasForeignKey("LessonPlanId");
            lcb.Property<Guid>("Id");
            lcb.HasKey("Id", "LessonPlanId");
        });

        builder.OwnsMany(lp => lp.ToDos, ltd =>
        {
            ltd.ToTable("TodoItem");
            ltd.WithOwner().HasForeignKey("LessonPlanId");
            ltd.Property<Guid>("Id");
            ltd.HasKey("Id", "LessonPlanId");
        });
    }
}