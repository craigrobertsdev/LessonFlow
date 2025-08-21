using LessonFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class TermDatesConfiguration : IEntityTypeConfiguration<SchoolTerm>
{
    public void Configure(EntityTypeBuilder<SchoolTerm> builder)
    {
        builder.Property<Guid>("Id");
        builder.HasKey("Id");
    }
}