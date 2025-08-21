using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

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

    public class YearLevelConfiguration : IEntityTypeConfiguration<YearLevel>
    {
        public void Configure(EntityTypeBuilder<YearLevel> builder)
        {
            builder.ToTable("YearLevels");
            builder.Property<Guid>("Id");
            builder.HasKey("Id");

            builder.Ignore(yl => yl.Name);

            builder.Property(yl => yl.YearLevelValue)
                .HasConversion(
                    v => (int)v,
                    v => (YearLevelValue)v);

            builder.OwnsMany(yl => yl.Capabilities, cb =>
            {
                cb.ToTable("Capabilities");
                cb.Property<Guid>("Id");
                cb.HasKey("Id");
                cb.WithOwner().HasForeignKey("YearLevelId");
            });

            builder.OwnsMany(yl => yl.Dispositions, db =>
            {
                db.ToTable("Dispositions");
                db.Property<Guid>("Id");
                db.HasKey("Id");
                db.WithOwner().HasForeignKey("YearLevelId");
            });

            builder.OwnsMany(yl => yl.ConceptualOrganisers, cb =>
            {
                cb.ToTable("ConceptualOrganisers");
                cb.Property<Guid>("Id");
                cb.HasKey("Id");
                cb.WithOwner().HasForeignKey("YearLevelId");

                cb.OwnsMany(cb => cb.ContentDescriptions, cdb =>
                {
                    cdb.ToTable("ContentDescriptions");
                    cdb.HasKey(cd => cd.Id);
                    cdb.WithOwner(cd => cd.ConceptualOrganiser).HasForeignKey("ConceptualOrganiserId");
                });
            });
        }
    }
}