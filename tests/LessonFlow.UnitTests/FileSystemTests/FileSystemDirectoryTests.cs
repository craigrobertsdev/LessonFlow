using LessonFlow.Domain.Curriculum;
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
        await parent.InitialiseAsync();

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
        await rootDir.InitialiseAsync();

        // Assert
        Assert.Equal(resources[0].Id, rootDir.Resources.First().Id);
        Assert.Equal(resources[1].Id, childDir.Resources.First().Id);
        Assert.Equal(resources[2].Id, nestedChildDir.Resources.First().Id);
    }

    [Fact]
    public void WhenFirstCreated_ExpandedShouldBeFalse()
    {
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        Assert.False(dir.IsExpanded);
    }

    [Fact]
    public async Task WhenSubjectPassed_ShouldExpandSubjectDirectory()
    {
        var maths = new Subject([], "Mathematics");
        var english = new Subject([], "English");
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object, initialSubjectId: maths.Id);
        var dir = new FileSystemDirectory("Parent", fs, null, subject: maths);
        var dir2 = new FileSystemDirectory("Mathematics", fs, null, subject: english);
        mockRepo.Setup(r => r.GetDirectories(fs.Id)).ReturnsAsync([dir, dir2]);

        await fs.InitialiseAsync();

        Assert.True(dir.IsExpanded);
        Assert.True(dir.IsInitialised);
        Assert.Equal(maths, dir.Subject);
        mockRepo.Verify(r => r.GetDirectories(fs.Id), Times.Once);
        Assert.False(dir2.IsExpanded);
        Assert.False(dir2.IsInitialised);
        Assert.Equal(english, dir2.Subject);
    }

    [Fact]
    public async Task WhenSubjectPassed_OnlyFirstLevelDirectoriesShouldBeExpanded()
    {
        var maths = new Subject([], "Mathematics");
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object, initialSubjectId: maths.Id);
        var dir = new FileSystemDirectory("Parent", fs, null, subject: maths);
        var childDir = new FileSystemDirectory("Child", fs, dir);
        mockRepo.Setup(r => r.GetDirectories(fs.Id)).ReturnsAsync([dir]);

        await fs.InitialiseAsync();

        Assert.True(dir.IsExpanded);
        Assert.True(dir.IsInitialised);
        Assert.False(childDir.IsExpanded);
        Assert.True(childDir.IsInitialised);
    }

    [Fact]
    public async Task Initialise_WhenSubjectPassed_ShouldLoadResourcesForSubjectDirectory()
    {
        var maths = new Subject([], "Mathematics");
        var resource = CreateResource("Parent");
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object, initialSubjectId: maths.Id);
        var dir = new FileSystemDirectory("Parent", fs, null, subject: maths);
        var childDir = new FileSystemDirectory("Child", fs, dir, subject: maths);
        var childDirResource = CreateResource("Child");
        var nestedChildDir = new FileSystemDirectory("NestedChild", fs, childDir, subject: maths);
        var nestedChildDirResource = CreateResource("NestedChild");

        mockRepo.Setup(r => r.GetDirectories(fs.Id)).ReturnsAsync([dir]);
        mockRepo.Setup(r => r.GetResources(dir)).ReturnsAsync([resource]);
        mockRepo.Setup(r => r.GetResources(childDir)).ReturnsAsync([childDirResource]);
        mockRepo.Setup(r => r.GetResources(nestedChildDir)).ReturnsAsync([nestedChildDirResource]);

        await fs.InitialiseAsync();

        Assert.Single(dir.Resources);
        Assert.Equal(resource.Id, dir.Resources.First().Id);
        Assert.Single(childDir.Resources);
        Assert.Equal(childDirResource.Id, childDir.Resources.First().Id);
        Assert.Single(nestedChildDir.Resources);
        Assert.Equal(nestedChildDirResource.Id, nestedChildDir.Resources.First().Id);
    }

    [Fact]
    public async Task CreateSubDirectory_ShouldStoreSubDirectoryInDatabaseAndAssociateWithParent()
    {
        // Arrange 
        var mockRepo = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, mockRepo.Object);
        var parentDirectory = new FileSystemDirectory("Parent", fileSystem, null);

        Assert.Empty(parentDirectory.Children);

        // Act
        await parentDirectory.CreateSubDirectoryAsync("SubDirectory");

        // Assert
        mockRepo.Verify(r => r.UpdateDirectory(parentDirectory), Times.Once);
        Assert.Single(parentDirectory.Children);
        Assert.Equal("SubDirectory", parentDirectory.Children[0].Name);
        Assert.Equal(parentDirectory, parentDirectory.Children[0].ParentDirectory);
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
    public async Task CreateSubDirectory_ShouldThrowException_WhenNameIsInvalid(string invalidName)
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, mockRepo.Object);
        var parentDirectory = new FileSystemDirectory("Parent", fileSystem, null);
        
        var act = async () => await parentDirectory.CreateSubDirectoryAsync(invalidName);

        await Assert.ThrowsAsync<ArgumentException>(act);
        mockRepo.Verify(r => r.UpdateDirectory(It.IsAny<FileSystemDirectory>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsExpanded_ShouldBeSetFromDatabase(bool isExpanded)
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new FileSystemId(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null)
        {
            IsExpanded = isExpanded
        };
        mockRepo.Setup(r => r.GetDirectories(fs.Id)).ReturnsAsync([dir]);

        await fs.InitialiseAsync();

        Assert.Equal(isExpanded, fs.Directories.First().IsExpanded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Rename_ShouldAllowNameToBeNullOrEmpty(string? newName)
    {
        var fs = new FileSystem(new(Guid.NewGuid()), new Mock<IFileSystemRepository>().Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await dir.RenameAsync(newName);

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
        await dir.RenameAsync("newName");

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

        await Assert.ThrowsAsync<ArgumentException>(() => dir.RenameAsync(invalidName));
    }

    [Fact]
    public async Task Rename_ShouldNotUpdateDatabase_WhenNewNameIsSameAsCurrent()
    {
        var mockRepo = new Mock<IFileSystemRepository>();
        var fs = new FileSystem(new(Guid.NewGuid()), mockRepo.Object);
        var dir = new FileSystemDirectory("Parent", fs, null);

        await dir.RenameAsync("Parent");

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

        await Assert.ThrowsAsync<Exception>(() => dir.RenameAsync("NewName"));
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

    private static Resource CreateResource(string name = "Resource")
    {
        return new(Guid.NewGuid(), name, name, 1025, "/path/to/resource", ResourceType.Article);
    }
}