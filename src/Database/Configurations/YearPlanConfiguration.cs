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
        builder.HasKey(yp => yp.Id);

        builder.Property(yp => yp.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.YearPlanIdConverter());

        builder.HasMany(yp => yp.Students)
            .WithOne();

        builder.HasOne(yp => yp.TermPlanner)
            .WithOne()
            .HasForeignKey<TermPlanner>(tp => tp.YearPlanId);

        builder.HasOne<User>()
            .WithMany(u => u.YearPlans)
            .HasForeignKey(yp => yp.UserId);

        builder.HasMany(yp => yp.WeekPlanners)
            .WithOne()
            .HasForeignKey(wp => wp.YearPlanId);

        builder.HasOne(yp => yp.WeekPlannerTemplate)
            .WithOne()
            .HasForeignKey<YearPlan>(yp => yp.WeekPlannerTemplateId);
            //.HasForeignKey<WeekPlannerTemplate>("YearPlanId");

        builder.HasMany(yp => yp.SubjectsTaught)
            .WithMany();

        builder.Navigation(yp => yp.WeekPlannerTemplate).AutoInclude();
        builder.Navigation(yp => yp.Students).AutoInclude();
        builder.Navigation(yp => yp.SubjectsTaught).AutoInclude();
        builder.Navigation(yp => yp.WeekPlanners).AutoInclude();

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property(yp => yp.WorkingDays)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<DayOfWeek>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(yp => yp.YearLevelsTaught)
            .HasMaxLength(100)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<YearLevel>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<YearLevel>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
    }
}