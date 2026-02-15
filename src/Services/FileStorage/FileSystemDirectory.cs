using System.Text.RegularExpressions;
using LessonFlow.Domain.Resources;

namespace LessonFlow.Services.FileStorage;

/// <summary>
/// Represents a directory in the resource file system
/// </summary>
public partial class FileSystemDirectory
{
    public Guid Id { get; } = Guid.NewGuid();
    public FileSystem ContainingFileSystem { get; }
    
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
    
    public bool IsExpanded { get; set; } = true;

    
        [GeneratedRegex("^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$", RegexOptions.Compiled)]
    private partial Regex ValidNameRegex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDirectory"/> class
    /// </summary>
    /// <param name="name">The name of the directory</param>
    /// <param name="containingFileSystem"></param>
    /// <param name="parent">The parent directory, or null if this is a root directory</param>
    public FileSystemDirectory(string name, FileSystem containingFileSystem, FileSystemDirectory? parent)
    {
        Name = name;
        ContainingFileSystem = containingFileSystem;
        if (parent is not null)
        {
            ParentDirectory = parent;
            parent.Children.Add(this);
        }
    }

    public async Task Initialise()
    {
        try
        {
            var resources = await ContainingFileSystem.GetResources(this);
            Resources = resources;

            List<Task> childInitialisationTasks = [];
            foreach (var child in Children)
            {
                var t = child.Initialise();
                childInitialisationTasks.Add(t);
            }
            await Task.WhenAll(childInitialisationTasks);
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
    public async Task Rename(string? newName)
    {
        var oldName = Name;
        try
        {
            if (newName == Name) return;
            CheckNameValid(newName);

            Name = newName;

            await ContainingFileSystem.UpdateDirectory(this);
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
        
        if (!ValidNameRegex.IsMatch(name))
        {
            throw new ArgumentException("Directory name must only contain letters, numbers, and the characters '.', '_', and '-'. It must also not start or end with a special character.");
        }
    }

#pragma warning disable CS8618
    private FileSystemDirectory()
    {
    }
}