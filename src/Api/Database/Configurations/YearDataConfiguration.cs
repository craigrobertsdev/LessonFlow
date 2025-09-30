using System.Text.Json;
using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class YearDataConfiguration : IEntityTypeConfiguration<YearData>
{
    public void Configure(EntityTypeBuilder<YearData> builder)
    {
        builder.HasKey(yd => yd.Id);

        builder.Property(yd => yd.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.YearDataIdConverter());

        builder.HasMany(yd => yd.Students)
            .WithOne();

        builder.HasOne(yd => yd.TermPlanner)
            .WithOne()
            .HasForeignKey<TermPlanner>(tp => tp.YearDataId);

        builder.HasMany(yd => yd.LessonPlans)
            .WithOne(lp => lp.YearData);

        builder.HasOne<User>()
            .WithMany(u => u.YearDataHistory)
            .HasForeignKey(yd => yd.UserId);

        builder.HasMany(yd => yd.WeekPlanners)
            .WithOne(wp=> wp.YearData);

        builder.HasOne(yd => yd.WeekPlannerTemplate)
            .WithOne()
            .HasForeignKey<WeekPlannerTemplate>("YearDataId");

        builder.HasMany(yd => yd.SubjectsTaught)
            .WithMany();

        builder.Navigation(yd => yd.WeekPlannerTemplate).AutoInclude();
        builder.Navigation(yd => yd.Students).AutoInclude();
        builder.Navigation(yd => yd.SubjectsTaught).AutoInclude();

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property<List<DayOfWeek>>("_workingDays")
            .HasColumnName("WorkingDays")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<DayOfWeek>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property<List<YearLevelValue>>("_yearLevelsTaught")
            .HasColumnName("YearLevels")
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