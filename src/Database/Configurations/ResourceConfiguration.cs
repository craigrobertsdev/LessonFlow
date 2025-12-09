using System.Text.Json;
using LessonFlow.Database.Converters;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.ResourceIdConverter());

        builder.Property(r => r.FileName)
            .HasMaxLength(500);

        builder.Property(r => r.Link)
            .HasMaxLength(300);

        builder.HasMany(r => r.ConceptualOrganisers)
            .WithMany();

        builder.HasOne<User>()
            .WithMany(u => u.Resources)
            .HasForeignKey(r => r.UserId);

        builder.HasMany(r => r.Subjects)
            .WithMany();

        builder.HasMany(r => r.LessonPlans)
            .WithMany(lp => lp.Resources);

#pragma warning disable CS8600, CS8603, CS8604 // Converting null literal or possible null value to non-nullable type.
        builder.Property(r => r.YearLevels)
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