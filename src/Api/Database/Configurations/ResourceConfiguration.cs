using System.Text.Json;
using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.ResourceIdConverter());

        builder.Property(r => r.Name)
            .HasMaxLength(500);

        builder.Property(r => r.Url)
            .HasMaxLength(300);

        builder.Property(r => r.AssociatedStrands)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                null);

        builder.HasOne<User>()
            .WithMany(u => u.Resources)
            .HasForeignKey(r => r.UserId);

        builder.HasOne(r => r.Subject)
            .WithMany()
            .IsRequired();

        builder.HasMany(r => r.LessonPlans)
            .WithMany(lp => lp.Resources);

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property<List<YearLevelValue>>("_yearLevels")
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