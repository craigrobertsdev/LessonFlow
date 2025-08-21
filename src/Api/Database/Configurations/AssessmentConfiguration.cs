using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

internal class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.AssessmentIdConverter());

        builder.Property(a => a.YearLevel)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.PlanningNotes)
            .HasMaxLength(500);

        builder.Property(a => a.AssessmentType)
            .HasConversion<string>()
            .HasMaxLength(15);

        builder.HasOne(a => a.Subject)
            .WithMany()
            .IsRequired();

        builder.HasOne<Student>()
            .WithMany(s => s.Assessments)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.User)
            .WithMany()
            .IsRequired();

        builder.Property(a => a.YearLevel)
            .HasConversion<string>()
            .HasMaxLength(15);

        builder.OwnsOne(a => a.AssessmentResult, arb =>
        {
            arb.ToTable("AssessmentResults");
            arb.WithOwner().HasForeignKey("AssessmentId");
            arb.Property<Guid>("Id");
            arb.HasKey("Id", "AssessmentId");

            arb.Property(ar => ar.Comments)
                .HasMaxLength(1000);

            arb.OwnsOne(ar => ar.Grade, gb =>
            {
                gb.Property(g => g.Percentage)
                    .HasColumnName("Percentage")
                    .IsRequired();

                gb.Property(g => g.Grade)
                    .HasConversion<string>()
                    .HasMaxLength(10);
            });
        });
    }
}