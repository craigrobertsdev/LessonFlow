using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Services.FileStorage;

public class FileSystem
{
    private readonly List<FileSystemDirectory> _directoriesPendingDeletion = [];
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly SubjectId? _initialSubjectId;
    private readonly List<FileSystemDirectory> _selectedDirectories = [];
    private List<FileSystemDirectory> _directories = [];

    public FileSystem(FileSystemId id, IFileSystemRepository fileSystemRepository, SubjectId initialSubjectId = null)
    {
        Id = id;
        _fileSystemRepository = fileSystemRepository;
        _initialSubjectId = initialSubjectId;
    }

    public FileSystemId Id { get; private set; }
    public IReadOnlyList<FileSystemDirectory> Directories => _directories;
    public DirectorySelectionMode DirectorySelectionMode { get; set; } = DirectorySelectionMode.Single;
    public IEnumerable<FileSystemDirectory> SelectedDirectories => _selectedDirectories;

    public async Task InitialiseAsync()
    {
        try
        {
            var directories = await _fileSystemRepository.GetDirectories(Id);
            if (_initialSubjectId is null)
            {
                _directories = directories;
                return;
            }

            var dir = directories.FirstOrDefault(d => d.Subject?.Id == _initialSubjectId);
            if (dir is not null)
            {
                await dir.InitialiseAsync(true);
            }

            _directories = directories;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<FileSystemDirectory> CreateDirectory(string? name)
    {
        if (!string.IsNullOrEmpty(name) && !FileSystemUtils.ValidNameRegex.IsMatch(name))
        {
            throw new ArgumentException(
                "Directory name must only contain letters, numbers, and the characters '.', '_', and '-'. It must also not start or end with a special character.");
        }

        var directory = new FileSystemDirectory(name, this, null);
        CheckDirectoryNameConflict(directory, name);

        try
        {
            await _fileSystemRepository.AddDirectory(directory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

        _directories.Add(directory);
        return directory;
    }

    /// <summary>
    /// Searches the file system for a directory matching the provided selector function.
    /// The search is performed using a breadth-first approach, starting from the root
    /// directories and traversing down the hierarchy.
    /// If a matching directory is found, it is returned; otherwise, null is returned.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    private FileSystemDirectory? FindDirectory(Func<FileSystemDirectory, bool> selector)
    {
        var queue = new Queue<FileSystemDirectory>(_directories);
        foreach (var directory in _directories)
        {
            queue.Enqueue(directory);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (selector(current))
            {
                return current;
            }

            foreach (var subDir in current.SubDirectories)
            {
                queue.Enqueue(subDir);
            }
        }

        return null;
    }

    public async Task<List<Resource>> GetResourcesAsync(FileSystemDirectory directory)
    {
        try
        {
            return await _fileSystemRepository.GetResources(directory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }

    public async Task UpdateDirectoryAsync(FileSystemDirectory directory)
    {
        try
        {
            await _fileSystemRepository.UpdateDirectory(directory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public void SelectDirectory(FileSystemDirectory directory)
    {
        if (DirectorySelectionMode == DirectorySelectionMode.Single)
        {
            var previouslySelected = _selectedDirectories.FirstOrDefault(d => d == directory)?.IsSelected;
            foreach (var selectedDir in _selectedDirectories)
            {
                selectedDir.IsSelected = false;
            }

            _selectedDirectories.Clear();

            if (previouslySelected is not null && previouslySelected == true) return;

            directory.IsSelected = true;
            _selectedDirectories.Add(directory);

            return;
        }

        if (directory.IsSelected)
        {
            DeselectDirectory(directory);
        }
        else
        {
            directory.IsSelected = true;
            _selectedDirectories.Add(directory);
        }
    }

    public void DeselectDirectory(FileSystemDirectory directory)
    {
        directory.IsSelected = false;
        _selectedDirectories.Remove(directory);
    }

    public long GetTotalDirectoryFileSize()
    {
        var totalSize = 0L;
        foreach (var dir in Directories)
        {
            totalSize += dir.GetSizeRecursive();
        }

        return totalSize;
    }

    public async Task RenameDirectoryAsync(FileSystemDirectory directory, string newName)
    {
        CheckDirectoryNameConflict(directory, newName);

        await directory.RenameAsync(newName);
    }

    private void CheckDirectoryNameConflict(FileSystemDirectory directory, string? name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var conflict =
            FindDirectory(d => d.Name is not null && d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (conflict is not null)
        {
            throw new ArgumentException($"A directory with the name '{name}' already exists.");
        }
    }

    public async Task DeleteDirectoryAsync(FileSystemDirectory directory)
    {
        try
        {
            directory.MarkAsDeleted();
            await _fileSystemRepository.UpdateDirectory(directory);

            _directories.Remove(directory);
            _selectedDirectories.Remove(directory);

            _directoriesPendingDeletion.Add(directory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public async Task RestoreDirectoryAsync(FileSystemDirectory directory)
    {
        await directory.Restore();

        _directories.Add(directory);
        _directoriesPendingDeletion.Remove(directory);
    }

#pragma warning disable CS8618
    private FileSystem()
    {
    }
}