using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Persistence;
using Moq;

namespace LessonFlow.UnitTests.FileSystemTests;

public class FileSystemDirectoryTests
{
    [Fact]
    public void Initialise_ShouldStoreProperties()
    {
        // Act 
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        Assert.Equal("Parent", dir.Name);
        Assert.Null(dir.ParentDirectory);
    }

    [Fact]
    public void Initialise_ShouldStoreParentWhenProvidedOne()
    {
        // Arrange
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var parent = new FileSystemDirectory("Parent", fs, null);

        // Act
        var dir = new FileSystemDirectory("Child", fs, parent);

        // Assert
        Assert.Equal("Child", dir.Name);
        Assert.Equal(parent, dir.ParentDirectory);
    }

    [Fact]
    public async Task Initialise_WhenResourcesExist_ShouldLoadResources()
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var parent = new FileSystemDirectory("Parent", fs, null);
        var resource = new Resource(Guid.NewGuid(), "Resource", "Resource", 1024, "/path/to/resource",
            ResourceType.Article);
        resource.Directory = parent;

        mockRepo.Setup(r => r.GetResources(It.IsAny<FileSystemDirectory>())).ReturnsAsync([resource]);

        // Act
        await parent.Initialise();

        // Assert
        Assert.Single(parent.Resources);
        Assert.Equal(resource.Id, parent.Resources.First().Id);

        mockRepo.Verify(r => r.GetResources(parent), Times.Once);
    }

    [Fact]
    public async Task Initialise_WhenHasChildren_ShouldInitialiseChildren()
    {
        // Arrange
        var resources = new List<Resource>()
        {
            new(Guid.NewGuid(), "Resource", "Resource", 1024, "/path/to/resource", ResourceType.Article),
            new(Guid.NewGuid(), "Resource", "Resource", 1024, "/path/to/resource", ResourceType.Article),
            new(Guid.NewGuid(), "Resource", "Resource", 1024, "/path/to/resource", ResourceType.Article),
        };
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var rootDir = new FileSystemDirectory("Root", fs, null);
        var childDir = new FileSystemDirectory("Child", fs, rootDir);
        var nestedChildDir = new FileSystemDirectory("NestedChild", fs, childDir);
        mockRepo.Setup(r => r.GetResources(rootDir)).ReturnsAsync([resources[0]]);
        mockRepo.Setup(r => r.GetResources(childDir)).ReturnsAsync([resources[1]]);
        mockRepo.Setup(r => r.GetResources(nestedChildDir)).ReturnsAsync([resources[2]]);

        Assert.Empty(rootDir.Resources);
        Assert.Empty(childDir.Resources);
        Assert.Empty(nestedChildDir.Resources);

        // Act
        await rootDir.Initialise();

        // Assert
        Assert.Equal(resources[0].Id, rootDir.Resources.First().Id);
        Assert.Equal(resources[1].Id, childDir.Resources.First().Id);
        Assert.Equal(resources[2].Id, nestedChildDir.Resources.First().Id);
    }

    [Fact]
    public async Task Initialise_WhenFirstCreated_ExpandedShouldBeTrue()
    {
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        Assert.True(dir.IsExpanded);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsExpanded_ShouldBeSetFromDatabase(bool isExpanded)
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new FileSystemId(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null);
        dir.IsExpanded = isExpanded;
        mockRepo.Setup(r => r.GetDirectories(fs.Id)).ReturnsAsync([dir]); 

        await fs.Initialise();
        
        Assert.Equal(isExpanded, fs.Directories.First().IsExpanded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Rename_ShouldAllowNameToBeNullOrEmpty(string newName)
    {
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await dir.Rename(newName);

        Assert.Equal(newName, dir.Name);
    }

    [Fact]
    public async Task Rename_ShouldUpdateDatabaseWhenSuccessful()
    {
        // Arrange
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        // Act
        await dir.Rename("newName");

        // Assert
        Assert.Equal("newName", dir.Name);
        mockRepo.Verify(r => r.UpdateDirectory(dir), Times.Once);
    }

    [Theory]
    [InlineData("Invalid/Name")]
    [InlineData("Invalid\\Name")]
    [InlineData("Invalid/Name/")]
    [InlineData(".")]
    [InlineData(",")]
    [InlineData(".Invalid")]
    [InlineData("Invalid.")]
    [InlineData("Inv..alid")]
    public async Task Rename_ShouldThrowException_WhenNewNameIsInvalid(string invalidName)
    {
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await Assert.ThrowsAsync<ArgumentException>(() => dir.Rename(invalidName));
    }

    [Fact]
    public async Task Rename_ShouldNotUpdateDatabase_WhenNewNameIsSameAsCurrent()
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await dir.Rename("Parent");

        mockRepo.Verify(r => r.UpdateDirectory(It.IsAny<FileSystemDirectory>()), Times.Never);
    }

    [Fact]
    public async Task Rename_ShouldThrowException_WhenRepositoryUpdateFails()
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        mockRepo.Setup(r => r.UpdateDirectory(It.IsAny<FileSystemDirectory>()))
            .ThrowsAsync(new Exception("Database error"));
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await Assert.ThrowsAsync<Exception>(() => dir.Rename("NewName"));
        Assert.Equal("Parent", dir.Name);
    }

    [Fact]
    public void GetRootDirectory_ShouldReturnNullForRootDirectories()
    {
        // Arrange
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var rootDir = new FileSystemDirectory("Root", fs, null);

        // Act
        var result = rootDir.GetRootDirectory();

        // Assert
        Assert.Equal(rootDir, result);
    }

    [Fact]
    public void GetRootDirectory_ShouldReturnParentForNonRootDirectories()
    {
        // Arrange
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var parentDir = new FileSystemDirectory("Parent", fs, null);
        var childDir = new FileSystemDirectory("Child", fs, parentDir);

        // Act
        var result = childDir.GetRootDirectory();

        // Assert
        Assert.Equal(parentDir, result);
    }

    [Fact]
    public void GetRootDirectory_ShouldReturnRootDirectoryForNestedDirectories()
    {
        // Arrange
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var rootDir = new FileSystemDirectory("Root", fs, null);
        var childDir = new FileSystemDirectory("Child", fs, rootDir);
        var nestedChildDir = new FileSystemDirectory("NestedChild", fs, childDir);

        // Act
        var result = nestedChildDir.GetRootDirectory();

        // Assert
        Assert.Equal(rootDir, result);
    }
}