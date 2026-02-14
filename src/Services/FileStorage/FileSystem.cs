using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Services.FileStorage;

public class FileSystem
{
    private readonly FileSystemId _id;
    private readonly IResourceRepository _resourceRepository;
    private List<ResourceDirectory> _directories = [];
    public IReadOnlyList<ResourceDirectory> Directories => _directories;
    
    public FileSystem(FileSystemId id, IResourceRepository resourceRepository)
    {
        _id = id;
        _resourceRepository = resourceRepository;
    }

    public async Task Initialise()
    {
        try
        {
            var directories = await _resourceRepository.GetDirectories(_id);
            _directories = directories;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}