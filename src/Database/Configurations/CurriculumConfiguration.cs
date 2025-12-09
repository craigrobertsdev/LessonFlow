using LessonFlow.Database.Converters;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class CurriculumConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.CurriculumSubjectIdConverter());

        builder.Property(s => s.Name)
            .HasMaxLength(50);

        builder.HasMany(s => s.YearLevels)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    public class YearLevelConfiguration : IEntityTypeConfiguration<CurriculumYearLevel>
    {
        public void Configure(EntityTypeBuilder<CurriculumYearLevel> builder)
        {
            builder.ToTable("YearLevels");
            builder.Property<Guid>("Id");
            builder.HasKey("Id");

            builder.Ignore(yl => yl.Name);

            builder.Property(yl => yl.YearLevelValue)
                .HasConversion(
                    v => (int)v,
                    v => (YearLevel)v);

            builder.HasMany(yl => yl.Capabilities)
                .WithOne()
                .HasForeignKey("YearLevelId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(yl => yl.Dispositions)
                .WithOne()
                .HasForeignKey("YearLevelId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(yl => yl.ConceptualOrganisers)
                .WithOne()
                .HasForeignKey("YearLevelId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

public class CapabilityConfiguration : IEntityTypeConfiguration<Capability>
{
    public void Configure(EntityTypeBuilder<Capability> builder)
    {
        builder.ToTable("Capabilities");
        builder.Property<Guid>("Id");
        builder.HasKey("Id");
    }
}

public class DispositionConfiguration : IEntityTypeConfiguration<Disposition>
{
    public void Configure(EntityTypeBuilder<Disposition> builder)
    {
        builder.ToTable("Dispositions");
        builder.Property<Guid>("Id");
        builder.HasKey("Id");
    }
}

public class ConceptualOrganiserConfiguration : IEntityTypeConfiguration<ConceptualOrganiser>
{
    public void Configure(EntityTypeBuilder<ConceptualOrganiser> builder)
    {
        builder.ToTable("ConceptualOrganisers");
        builder.HasKey(co => co.Id);

        builder.HasMany(co => co.ContentDescriptions)
            .WithOne(cd => cd.ConceptualOrganiser)
            .HasForeignKey("ConceptualOrganiserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContentDescriptionConfiguration : IEntityTypeConfiguration<ContentDescription>
{
    public void Configure(EntityTypeBuilder<ContentDescription> builder)
    {
        builder.ToTable("ContentDescriptions");
        builder.HasKey(cd => cd.Id);
    }
}