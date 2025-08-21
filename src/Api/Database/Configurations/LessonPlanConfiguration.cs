using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.HasKey(lp => lp.Id);

        builder.Property(lp => lp.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.LessonPlanIdConverter());

        builder.HasOne(lp => lp.YearData)
            .WithMany(yd => yd.LessonPlans)
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
    }
}