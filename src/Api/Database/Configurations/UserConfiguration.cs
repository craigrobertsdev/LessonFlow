using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
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

        // Fix: configure as shared PK one-to-one; FK is on AccountSetupState.Id
        builder.HasOne(u => u.AccountSetupState)
            .WithOne()
            .HasForeignKey<AccountSetupState>(s => s.Id);
    }
}

public class AccountSetupStateConfiguration : IEntityTypeConfiguration<AccountSetupState>
{
    public void Configure(EntityTypeBuilder<AccountSetupState> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne<User>()
            .WithOne(u => u.AccountSetupState)
            .HasForeignKey<AccountSetupState>(s => s.Id);

        // Fix: Configure the relationship properly with AccountSetupState owning WeekPlannerTemplate
        // Add a shadow property for the foreign key
        builder.HasOne(a => a.WeekPlannerTemplate)
            .WithOne()
            .HasForeignKey<WeekPlannerTemplate>("AccountSetupStateId");

        builder.Property(x => x.YearLevelsTaught)
            .HasConversion(
                v => string.Join(',', v.Select(y => y.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(y => Enum.Parse<YearLevelValue>(y))
                      .ToList())
            .HasColumnName("YearLevelsTaught")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.SubjectsTaught)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("SubjectsTaught")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.WorkingDays)
            .HasConversion(
                v => string.Join(',', v.Select(d => d.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(d => Enum.Parse<DayOfWeek>(d))
                      .ToList())
            .HasColumnName("WorkingDays")
            .HasMaxLength(128)
            .IsRequired();

        builder.Navigation(x => x.WeekPlannerTemplate).AutoInclude();
    }
}
