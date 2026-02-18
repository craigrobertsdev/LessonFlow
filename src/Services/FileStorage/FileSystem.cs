using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Services.FileStorage;

public class FileSystem
{
    public FileSystemId Id { get; private set; }
    private readonly IFileSystemRepository _fileSystemRepository;
    private List<FileSystemDirectory> _directories = [];
    private readonly SubjectId? _initialSubjectId;
    public IReadOnlyList<FileSystemDirectory> Directories => _directories;
    public DirectorySelectionMode DirectorySelectionMode { get; set; } = DirectorySelectionMode.Single;
    private readonly List<FileSystemDirectory> _selectedDirectories = [];
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
        
        _directories.Add(directory);
        return directory;
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
            directory.IsSelected = false;
            _selectedDirectories.Remove(directory);
        }
        else
        {
            directory.IsSelected = true;
            _selectedDirectories.Add(directory);
        }
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

    public FileSystem(FileSystemId id, IFileSystemRepository fileSystemRepository, SubjectId initialSubjectId = null)
    {
        Id = id;
        _fileSystemRepository = fileSystemRepository;
        _initialSubjectId = initialSubjectId;
    }

#pragma warning disable CS8618
    private FileSystem()
    {
    }
}