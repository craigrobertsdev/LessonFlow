using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class SchoolEventConfiguration : IEntityTypeConfiguration<SchoolEvent>
{
    public void Configure(EntityTypeBuilder<SchoolEvent> builder)
    {
        builder.HasKey(se => se.Id);

        builder.Property(se => se.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.SchoolEventIdConverter());

        builder.Property(se => se.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(se => se.Location)
            .WithMany();
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.Property<Guid>("Id");
        builder.HasKey("Id");
    }
}