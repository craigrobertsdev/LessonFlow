using LessonFlow.Api.Database.Converters;
using LessonFlow.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasMany(u => u.YearDataHistory)
            .WithOne()
            .HasForeignKey(u => u.UserId);
    }
}