using System.Reflection;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
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
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());

        // Act
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Assert
        Assert.Empty(fileSystem.Directories);
    }

    [Fact]
    public async Task Initialise_ShouldLoadDirectories()
    {
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fs = new FileSystem(fsId, resourceRepository.Object);
        var top = new FileSystemDirectory("Top", fs, null);
        List<FileSystemDirectory> directories = [top];
        top.SubDirectories.Add(new("Nested", fs, top));
        typeof(FileSystem).GetField("_directories", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(fs,
            directories);

        resourceRepository.Setup(repo => repo.GetDirectories(fsId))
            .ReturnsAsync(directories);
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Act
        await fileSystem.InitialiseAsync();

        // Assert
        Assert.Equal(fs.Directories.Count, fileSystem.Directories.Count);
        Assert.Equal(fs.Directories[0], fileSystem.Directories[0]);
        Assert.Equal(fs.Directories[0].SubDirectories[0], fileSystem.Directories[0].SubDirectories[0]);
    }

    [Fact]
    public async Task Initialise_WhenNoChildrenExists_ShouldNotLoadDirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        resourceRepository.Setup(repo => repo.GetDirectories(It.IsAny<FileSystemId>()))
            .ReturnsAsync(new List<FileSystemDirectory>());
        var fileSystem = new FileSystem(new FileSystemId(Guid.NewGuid()), resourceRepository.Object);

        // Act
        await fileSystem.InitialiseAsync();

        // Assert
        Assert.Empty(fileSystem.Directories);
    }

    [Fact]
    public void SelectDirectory_WhenCalled_ShouldAddDirectoryToSelectedDirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = new FileSystemDirectory("Test Directory", fileSystem, null);

        // Act
        fileSystem.SelectDirectory(directory);

        // Assert
        Assert.Equal(DirectorySelectionMode.Single, fileSystem.DirectorySelectionMode);
        Assert.True(directory.IsSelected);
        Assert.Contains(directory, fileSystem.SelectedDirectories);
    }

    [Fact]
    public void SelectDirectory_WhenCalledTwiceOnSameDirectory_ShouldRemoveDirectoryFromSelectedDirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = new FileSystemDirectory("Test Directory", fileSystem, null);

        // Act
        fileSystem.SelectDirectory(directory);
        fileSystem.SelectDirectory(directory);

        // Assert
        Assert.Equal(DirectorySelectionMode.Single, fileSystem.DirectorySelectionMode);
        Assert.False(directory.IsSelected);
        Assert.Empty(fileSystem.SelectedDirectories);
    }

    [Fact]
    public void SelectDirectory_WhenCalledAndDirectorySelectionModeIsSingle_ShouldDeselectOtherDirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory1 = new FileSystemDirectory("Directory 1", fileSystem, null);
        var directory2 = new FileSystemDirectory("Directory 2", fileSystem, null);

        // Act
        fileSystem.SelectDirectory(directory1);
        fileSystem.SelectDirectory(directory2);

        // Assert
        Assert.Equal(DirectorySelectionMode.Single, fileSystem.DirectorySelectionMode);
        Assert.False(directory1.IsSelected);
        Assert.True(directory2.IsSelected);
        Assert.DoesNotContain(directory1, fileSystem.SelectedDirectories);
        Assert.Contains(directory2, fileSystem.SelectedDirectories);
    }

    [Fact]
    public void SelectDirectory_WhenCalledAndDirectorySelectionModeIsMultiple_ShouldAllowMultipleSelectedDirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object)
        {
            DirectorySelectionMode = DirectorySelectionMode.Multiple
        };
        var directory1 = new FileSystemDirectory("Directory 1", fileSystem, null);
        var directory2 = new FileSystemDirectory("Directory 2", fileSystem, null);

        // Act
        fileSystem.SelectDirectory(directory1);
        fileSystem.SelectDirectory(directory2);

        // Assert
        Assert.Equal(DirectorySelectionMode.Multiple, fileSystem.DirectorySelectionMode);
        Assert.True(directory1.IsSelected);
        Assert.True(directory2.IsSelected);
        Assert.Contains(directory1, fileSystem.SelectedDirectories);
        Assert.Contains(directory2, fileSystem.SelectedDirectories);
    }

    [Fact]
    public void SelectDirectory_WhenCalledAndDirectorySelectionModeIsMultiple_ShouldDeselectDirectoryWhenSelectedAgain()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object)
        {
            DirectorySelectionMode = DirectorySelectionMode.Multiple
        };
        var directory1 = new FileSystemDirectory("Directory 1", fileSystem, null);
        var directory2 = new FileSystemDirectory("Directory 2", fileSystem, null);

        // Act
        fileSystem.SelectDirectory(directory1);
        fileSystem.SelectDirectory(directory2);
        fileSystem.SelectDirectory(directory1); // Deselect directory1

        // Assert
        Assert.Equal(DirectorySelectionMode.Multiple, fileSystem.DirectorySelectionMode);
        Assert.False(directory1.IsSelected);
        Assert.True(directory2.IsSelected);
        Assert.DoesNotContain(directory1, fileSystem.SelectedDirectories);
        Assert.Contains(directory2, fileSystem.SelectedDirectories);
    }

    [Fact]
    public async Task CreateDirectory_WhenCalled_ShouldCreateDirectoryAndAddToFileSystem()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Act
        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Assert
        Assert.NotNull(directory);
        Assert.Equal("Test Directory", directory.Name);
        Assert.Contains(directory, fileSystem.Directories);
    }

    [Fact]
    public async Task CreateDirectory_WhenCalledWithInvalidName_ShouldThrowArgumentException()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var invalidName = "Invalid/Directory\\Name";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fileSystem.CreateDirectory(invalidName));
    }

    [Fact]
    public async Task CreateDirectory_WhenCalledWithDuplicateName_ShouldThrowArgumentException()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directoryName = "Test Directory";
        await fileSystem.CreateDirectory(directoryName);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fileSystem.CreateDirectory(directoryName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("Name")]
    public async Task CreateDirectory_WhenCalledWithDirectoriesThatHaveEmptyNames_ShouldCreateDirectory(string? newName)
    {
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        await fileSystem.CreateDirectory(string.Empty);

        // Act
        var newDirectory = await fileSystem.CreateDirectory(newName);

        // Assert
        Assert.NotNull(newDirectory);
        Assert.Equal(newName, newDirectory.Name);
    }

    [Fact]
    public async Task CreateDirectory_WhenCalled_ShouldUpdateDatabaseWithDirectory()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Act
        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Assert
        resourceRepository.Verify(repo => repo.AddDirectory(directory), Times.Once);
    }

    [Fact]
    public async Task CreateDirectory_WhenCalledAndDatabaseThrowsException_ShouldNotAddDirectoryToFileSystem()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        resourceRepository.Setup(repo => repo.AddDirectory(It.IsAny<FileSystemDirectory>()))
            .ThrowsAsync(new Exception("Database error"));
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => fileSystem.CreateDirectory("Test Directory"));

        // Assert
        Assert.Empty(fileSystem.Directories);
    }

    [Fact]
    public async Task GetTotalDirectoryFileSize_WhenCalled_ShouldReturnTotalSizeOfAllFilesInDirectoryAndSubdirectories()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var dir = await fileSystem.CreateDirectory("Test Directory");
        var subDirectory = new FileSystemDirectory("Sub Directory", fileSystem, dir);

        var resource1 = new Resource(Guid.NewGuid(), "Resource 1", "Resource 1", 1000, string.Empty,
            ResourceType.Video);
        var resource2 = new Resource(Guid.NewGuid(), "Resource 3", "Resource 2", 2000, string.Empty,
            ResourceType.Article);
        var resource3 = new Resource(Guid.NewGuid(), "Resource 3", "Resource 3", 3000, string.Empty,
            ResourceType.Assessment);

        dir.Resources.Add(resource1);
        subDirectory.Resources.Add(resource2);
        subDirectory.Resources.Add(resource3);

        // Act
        var totalSize = fileSystem.GetTotalDirectoryFileSize();

        // Assert
        Assert.Equal(6000, totalSize);
    }

    [Fact]
    public async Task RenameDirectory_WhenCalled_ShouldUpdateDirectoryName()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Old Name");

        // Act
        await fileSystem.RenameDirectoryAsync(directory, "New Name");

        // Assert
        Assert.Equal("New Name", directory.Name);
    }

    [Fact]
    public async Task RenameDirectory_WhenCalled_ShouldUpdateDirectoryInDatabase()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Old Name");

        // Act
        await fileSystem.RenameDirectoryAsync(directory, "New Name");

        // Assert
        resourceRepository.Verify(repo => repo.UpdateDirectory(directory), Times.Once);
    }

    [Fact]
    public async Task RenameDirectory_WhenCalledAndDatabaseThrowsException_ShouldNotUpdateDirectoryName()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        resourceRepository.Setup(repo => repo.UpdateDirectory(It.IsAny<FileSystemDirectory>()))
            .ThrowsAsync(new Exception("Database error"));
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Old Name");

        // Act
        await Assert.ThrowsAsync<Exception>(() => fileSystem.RenameDirectoryAsync(directory, "New Name"));

        // Assert
        Assert.Equal("Old Name", directory.Name);
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
    public async Task RenameDirectory_WhenCalledWithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Old Name");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fileSystem.RenameDirectoryAsync(directory, invalidName));
    }

    [Fact]
    public async Task RenameDirectory_WhenCalledWithDuplicateName_ShouldThrowArgumentException()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory1 = await fileSystem.CreateDirectory("Directory 1");
        var directory2 = await fileSystem.CreateDirectory("Directory 2");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fileSystem.RenameDirectoryAsync(directory2, "Directory 1"));
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalled_ShouldRemoveDirectoryFromFileSystem()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Act
        await fileSystem.DeleteDirectoryAsync(directory);

        // Assert
        Assert.DoesNotContain(directory, fileSystem.Directories);
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalled_ShouldUpdateDirectoryInDatabase()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);

        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Act
        await fileSystem.DeleteDirectoryAsync(directory);

        // Assert
        resourceRepository.Verify(
            repo => repo.UpdateDirectory(It.Is<FileSystemDirectory>(d => d.Id == directory.Id && d.IsSoftDeleted)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalledAndDatabaseThrowsException_ShouldNotRemoveDirectoryFromFileSystem()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        resourceRepository.Setup(repo => repo.UpdateDirectory(It.IsAny<FileSystemDirectory>()))
            .ThrowsAsync(new Exception("Database error"));
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Act
        await Assert.ThrowsAsync<Exception>(() => fileSystem.DeleteDirectoryAsync(directory));

        // Assert
        Assert.Contains(directory, fileSystem.Directories);
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalled_ShouldSoftDeleteDirectory()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");

        // Act
        await fileSystem.DeleteDirectoryAsync(directory);

        // Assert
        Assert.True(directory.IsSoftDeleted);
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalled_ShouldMarkChildrenForDeletion()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var parentDirectory = await fileSystem.CreateDirectory("Parent Directory");
        var childDirectory = new FileSystemDirectory("Child Directory", fileSystem, parentDirectory);
        parentDirectory.SubDirectories.Add(childDirectory);

        // Act
        await fileSystem.DeleteDirectoryAsync(parentDirectory);

        // Assert
        Assert.True(parentDirectory.IsSoftDeleted);
        Assert.True(childDirectory.IsSoftDeleted);
    }

    [Fact]
    public async Task DeleteDirectory_WhenCalled_ShouldDeselectDeletedDirectoryAndChildren()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");
        var childDirectory = new FileSystemDirectory("Child Directory", fileSystem, directory);
        fileSystem.SelectDirectory(directory);
        fileSystem.SelectDirectory(childDirectory);

        // Act
        await fileSystem.DeleteDirectoryAsync(directory);

        // Assert
        Assert.False(directory.IsSelected);
        Assert.False(childDirectory.IsSelected);
        Assert.DoesNotContain(directory, fileSystem.SelectedDirectories);
    }

    [Fact]
    public async Task RestoreDirectory_WhenCalled_ShouldUnmarkDirectoryAsDeleted()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");
        await fileSystem.DeleteDirectoryAsync(directory);

        // Act
        await fileSystem.RestoreDirectoryAsync(directory);

        // Assert
        Assert.False(directory.IsSoftDeleted);
    }

    [Fact]
    public async Task RestoreDirectory_WhenCalled_ShouldUpdateDirectoryInDatabase()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");
        await fileSystem.DeleteDirectoryAsync(directory);

        // Act
        await fileSystem.RestoreDirectoryAsync(directory);

        // Assert
        resourceRepository.Verify(
            repo => repo.UpdateDirectory(It.Is<FileSystemDirectory>(d => d.Id == directory.Id && !d.IsSoftDeleted)),
            Times.Once);
    }

    [Fact]
    public async Task RestoreDirectory_WhenCalledAndDatabaseThrowsException_ShouldNotUnmarkDirectoryAsDeleted()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        resourceRepository.Setup(repo => repo.UpdateDirectory(It.IsAny<FileSystemDirectory>()))
            .ThrowsAsync(new Exception("Database error"));
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var directory = await fileSystem.CreateDirectory("Test Directory");
        directory.GetType().GetProperty("IsSoftDeleted")!.SetValue(directory, true);

        // Act
        await Assert.ThrowsAsync<Exception>(() => fileSystem.RestoreDirectoryAsync(directory));

        // Assert
        Assert.True(directory.IsSoftDeleted);
    }

    [Fact]
    public async Task RestoreDirectory_WhenCalled_ShouldUnmarkSubDirectoriesAsDeleted()
    {
        // Arrange
        var resourceRepository = new Mock<IFileSystemRepository>();
        var fsId = new FileSystemId(Guid.NewGuid());
        var fileSystem = new FileSystem(fsId, resourceRepository.Object);
        var parentDirectory = await fileSystem.CreateDirectory("Parent Directory");
        var childDirectory = new FileSystemDirectory("Child Directory", fileSystem, parentDirectory);
        parentDirectory.SubDirectories.Add(childDirectory);
        await fileSystem.DeleteDirectoryAsync(parentDirectory);

        // Act
        await fileSystem.RestoreDirectoryAsync(parentDirectory);

        // Assert
        Assert.False(parentDirectory.IsSoftDeleted);
        Assert.False(childDirectory.IsSoftDeleted);
    }
}