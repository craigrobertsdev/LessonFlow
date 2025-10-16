using LessonFlow.Database.Converters;
using LessonFlow.Domain.Calendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.CalendarIdConverter());

        builder.HasMany(tp => tp.SchoolEvents)
            .WithMany();
    }
}