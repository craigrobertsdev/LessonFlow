using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Services.FileStorage;

public class FileSystem
{
    public FileSystemId Id { get; private set; }
    private readonly IFileSystemRepository _fileSystemRepository;
    private List<FileSystemDirectory> _directories = [];
    public IReadOnlyList<FileSystemDirectory> Directories => _directories;

    public FileSystem(FileSystemId id, IFileSystemRepository fileSystemRepository)
    {
        Id = id;
        _fileSystemRepository = fileSystemRepository;
    }

    public async Task Initialise()
    {
        try
        {
            var directories = await _fileSystemRepository.GetDirectories(Id);
            _directories = directories;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public async Task<List<Resource>> GetResources(FileSystemDirectory directory)
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

    public async Task UpdateDirectory(FileSystemDirectory directory)
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

#pragma warning disable CS8618
    private FileSystem()
    {
    }
}