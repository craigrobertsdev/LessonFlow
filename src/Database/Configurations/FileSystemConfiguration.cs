using LessonFlow.Database.Converters;
using LessonFlow.Services.FileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LessonFlow.Database.Configurations;

public class FileSystemConfiguration : IEntityTypeConfiguration<FileSystem>
{
    public void Configure(EntityTypeBuilder<FileSystem> builder)
    {
        builder.ToTable("FileSystems");
        builder.HasKey(fs => fs.Id);
        builder.Property(fs => fs.Id)
            .HasColumnName("Id")
            .HasConversion(new StronglyTypedIdConverter.FileSystemIdConverter());

        builder.HasMany(fs => fs.Directories)
            .WithOne(d => d.ContainingFileSystem)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FileSystemDirectoryConfiguration : IEntityTypeConfiguration<FileSystemDirectory>
{
    public void Configure(EntityTypeBuilder<FileSystemDirectory> builder)
    {
        builder.ToTable("FileSystemDirectories");
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.ParentDirectory)
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}