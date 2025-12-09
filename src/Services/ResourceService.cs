using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Services.FileStorage;
using Microsoft.AspNetCore.Components.Forms;

namespace LessonFlow.Services;

public class ResourceService
{
    private readonly Guid _userId;
    private readonly List<Resource> _resourceCache = [];
    private readonly IStorageManager _storageManager;

    public IReadOnlyList<Resource> ResourceCache => _resourceCache.AsReadOnly();

    public ResourceService(IStorageManager storageManager, Guid userId)
    {
        _userId = userId;
        _storageManager = storageManager;
    }

    public async Task<Resource?> CreateResource(IBrowserFile file, string name, Subject subject, List<YearLevelValue> yearLevels, ResourceType type, List<ConceptualOrganiser>? topics, CancellationToken ct)
    {
        var safeFileName = Path.GetRandomFileName();
        try
        {
            var fileStream = file.OpenReadStream(cancellationToken: ct);
            var url = await _storageManager.Save(safeFileName, fileStream, ct);
            var resource = new Resource(_userId, name, url, subject, yearLevels, type, topics ?? []);
            _resourceCache.Add(resource);
            return resource;
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public void DeleteResource(Resource resource)
    {
        _resourceCache.Remove(resource);
    }

    public List<Resource> FilterResources(Subject subject, List<YearLevelValue>? yearLevels = null)
    {
        var resourceQuery = _resourceCache.AsQueryable();
        if (yearLevels is not null)
        {
            resourceQuery = resourceQuery.Where(r => r.YearLevels.Any(yl => yearLevels.Contains(yl)));
        }

        return resourceQuery.Where(r => r.Subject.Id == subject.Id).ToList();
    }
}
