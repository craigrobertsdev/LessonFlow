using System.Text.Json;
using LessonFlow.Database.Converters;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class TermPlannerConfiguration : IEntityTypeConfiguration<TermPlanner>
{
    public void Configure(EntityTypeBuilder<TermPlanner> builder)
    {
        builder.HasKey(tp => tp.Id);

        builder.Property(tp => tp.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.TermPlannerIdConverter());

        builder.Property(tp => tp.CalendarYear)
            .IsRequired();

        builder.HasOne<YearData>()
            .WithOne()
            .HasForeignKey<TermPlanner>(tp => tp.YearDataId)
            .OnDelete(DeleteBehavior.Cascade);

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property(tp => tp.YearLevels)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<YearLevelValue>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<YearLevelValue>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
    }
}

public class TermPlanConfiguration : IEntityTypeConfiguration<TermPlan>
{
    public void Configure(EntityTypeBuilder<TermPlan> builder)
    {
        builder.Property<Guid>("Id");
        builder.HasKey("Id");

        builder.HasMany(tp => tp.Subjects)
            .WithMany();

        builder.HasOne(tp => tp.TermPlanner)
            .WithMany(tp => tp.TermPlans)
            .HasForeignKey("TermPlannerId");
    }
}