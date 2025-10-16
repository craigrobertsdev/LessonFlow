using LessonFlow.Database.Converters;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.StudentIdConverter());

        builder.HasMany(s => s.Reports)
            .WithOne(s => s.Student)
            .IsRequired();

        builder.HasMany(s => s.Assessments)
            .WithOne(a => a.Student);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired();
    }
}