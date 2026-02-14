using System.Reflection;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Persistence;
using Moq;

namespace LessonFlow.UnitTests.FileSystemTests;

public class FileSystemTests
{
    [Fact]
    public void Initialise_ShouldHaveEmptyDirectoriesList()
    {
        var resourceRepository = new Mock<IResourceRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());

        // Act
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Assert
        Assert.Empty(fileSystem.Directories);
    }

    [Fact]
    public async Task Initialise_ShouldLoadDirectories()
    {
        var resourceRepository = new Mock<IResourceRepository>();
        var top = new ResourceDirectory("Top", null);
        List<ResourceDirectory> directories = [top];
            top.Children.Add(new("Nested", top));
        var fsId = new FileSystemId(Guid.NewGuid());
        var fs = new FileSystem(fsId, resourceRepository.Object);
        typeof(FileSystem).GetField("_directories", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(fs, directories);

        resourceRepository.Setup(repo => repo.GetDirectories(fsId))
            .ReturnsAsync(directories);
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        
        // Act
        await fileSystem.Initialise();
        
        // Assert
        Assert.Equal(fs.Directories.Count, fileSystem.Directories.Count);
        Assert.Equal(fs.Directories[0], fileSystem.Directories[0]);
        Assert.Equal(fs.Directories[0].Children[0], fileSystem.Directories[0].Children[0]);
    }
}