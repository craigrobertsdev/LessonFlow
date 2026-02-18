using System.Text.RegularExpressions;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;

namespace LessonFlow.Services.FileStorage;

/// <summary>
/// Represents a directory in the resource file system
/// </summary>
public partial class FileSystemDirectory
{
    public Guid Id { get; } = Guid.NewGuid();
    public FileSystem ContainingFileSystem { get; }
    public Subject? Subject { get; set; }

    /// <summary>
    /// Gets the child directories contained within this directory
    /// </summary>
    public List<FileSystemDirectory> Children = [];

    /// <summary>
    /// Gets the name of the directory
    /// </summary>
    public string? Name { get; private set; }

    public List<Resource> Resources { get; set; } = [];

    /// <summary>
    /// Gets the parent directory, or null if this is a root directory
    /// </summary>
    public FileSystemDirectory? ParentDirectory { get; }

    public bool IsExpanded { get; set; }
    public bool IsInitialised { get; private set; }
    public bool IsSelected { get; set; }

    public async Task InitialiseAsync(bool expand = false)
    {
        try
        {
            var getResourcesTask = ContainingFileSystem.GetResourcesAsync(this);

            List<Task> childInitialisationTasks = [];
            foreach (var child in Children)
            {
                var t = child.InitialiseAsync();
                childInitialisationTasks.Add(t);
            }

            await Task.WhenAll(childInitialisationTasks);

            var resources = await getResourcesTask;
            Resources = resources;

            IsExpanded = expand;
            IsInitialised = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public FileSystemDirectory GetRootDirectory()
    {
        if (ParentDirectory == null)
        {
            return this;
        }

        return ParentDirectory.GetRootDirectory();
    }

    /// <summary>
    /// Renames the directory
    /// </summary>
    /// <param name="newName">The new name for the directory</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newName"/> is null or empty</exception>
    public async Task RenameAsync(string? newName)
    {
        var oldName = Name;
        try
        {
            if (newName == Name) return;
            CheckNameValid(newName);

            Name = newName;

            await ContainingFileSystem.UpdateDirectoryAsync(this);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        catch (Exception e)
        {
            Name = oldName;
            throw;
        }
    }

    private void CheckNameValid(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (!FileSystemUtils.ValidNameRegex.IsMatch(name))
        {
            throw new ArgumentException(
                "Directory name must only contain letters, numbers, and the characters '.', '_', and '-'. It must also not start or end with a special character.");
        }
    }

    public async Task CreateSubDirectoryAsync(string name)
    {
        var subDirectory = new FileSystemDirectory(name, ContainingFileSystem, this);
        try
        {
            await ContainingFileSystem.UpdateDirectoryAsync(this);
        }
        catch (Exception e)
        {
            Children.Remove(subDirectory);
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public long GetSizeRecursive()
    {
        var size = 0L;
        size += Resources.Sum(r => r.FileSize);
        
        if (Children.Count == 0)
        {
            return size;
        }

        foreach (var child in Children)
        {
            size += child.GetSizeRecursive();
        }

        return size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDirectory"/> class. If a parent directory is provided, the new directory will automatically be added to the parent's children.
    /// </summary>
    /// <param name="name">The name of the directory</param>
    /// <param name="containingFileSystem"></param>
    /// <param name="parent">The parent directory, or null if this is a root directory</param>
    /// <param name="subject">The subject the directory is related to. </param>
    public FileSystemDirectory(string? name, FileSystem containingFileSystem, FileSystemDirectory? parent,
        Subject? subject = null)
    {
        Name = name;
        ContainingFileSystem = containingFileSystem;
        Subject = subject;
        if (parent is not null)
        {
            ParentDirectory = parent;
            parent.Children.Add(this);
        }
    }

#pragma warning disable CS8618
    private FileSystemDirectory()
    {
    }
}