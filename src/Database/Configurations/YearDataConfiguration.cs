using System.Text.Json;
using LessonFlow.Database.Converters;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class YearPlanConfiguration : IEntityTypeConfiguration<YearPlan>
{
    public void Configure(EntityTypeBuilder<YearPlan> builder)
    {
        builder.HasKey(yd => yd.Id);

        builder.Property(yd => yd.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.YearPlanIdConverter());

        builder.HasMany(yd => yd.Students)
            .WithOne();

        builder.HasOne(yd => yd.TermPlanner)
            .WithOne()
            .HasForeignKey<TermPlanner>(tp => tp.YearPlanId);

        builder.HasOne<User>()
            .WithMany(u => u.YearPlanHistory)
            .HasForeignKey(yd => yd.UserId);

        builder.HasMany(yd => yd.WeekPlanners)
            .WithOne()
            .HasForeignKey(wp => wp.YearPlanId);

        builder.HasOne(yd => yd.WeekPlannerTemplate)
            .WithOne()
            .HasForeignKey<WeekPlannerTemplate>("YearPlanId");

        builder.HasMany(yd => yd.SubjectsTaught)
            .WithMany();

        builder.Navigation(yd => yd.WeekPlannerTemplate).AutoInclude();
        builder.Navigation(yd => yd.Students).AutoInclude();
        builder.Navigation(yd => yd.SubjectsTaught).AutoInclude();
        builder.Navigation(yd => yd.WeekPlanners).AutoInclude();

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property(yd => yd.WorkingDays)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<DayOfWeek>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(yd => yd.YearLevelsTaught)
            .HasMaxLength(100)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<YearLevelValue>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<YearLevelValue>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
    }
}