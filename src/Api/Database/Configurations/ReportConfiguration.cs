using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Reports;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.ReportIdConverter());

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Subject)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Student)
            .WithMany(s => s.Reports)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(r => r.YearLevel)
            .HasConversion<string>()
            .HasMaxLength(15);

        builder.OwnsMany(r => r.ReportComments, cb => {
            cb.ToTable("ReportComments");
            cb.WithOwner().HasForeignKey("ReportId");
            cb.Property<Guid>("Id");
            cb.HasKey("Id");

            cb.Property(c => c.Comments)
                .HasMaxLength(500);

            cb.Property(c => c.Grade)
                .HasConversion<string>()
                .HasMaxLength(10);
        });
    }
}