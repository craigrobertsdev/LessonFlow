using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace LessonFlow.Services;

public class ResourceService
{
    private readonly Guid _userId;
    private readonly List<Resource> _resourceCache = [];
    private readonly IStorageManager _storageManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWorkFactory _uowFactory;

    public IReadOnlyList<Resource> ResourceCache => _resourceCache.AsReadOnly();

    public ResourceService(IStorageManager storageManager, IUserRepository userRepository, IUnitOfWorkFactory uowFactory, Guid userId)
    {
        _userId = userId;
        _storageManager = storageManager;
        _userRepository = userRepository;
        _uowFactory = uowFactory;
    }

    public async Task<Resource?> CreateResource(IBrowserFile file, string displayName, List<Subject> subjects, List<YearLevelValue> yearLevels, ResourceType resourceType, List<ConceptualOrganiser>? topics, CancellationToken ct)
    {
        var safeFileName = Path.GetRandomFileName();
        try
        {
            var fileStream = file.OpenReadStream(cancellationToken: ct);
            var response = await _storageManager.Save(safeFileName, fileStream, ct);
            var resource = new Resource(_userId, file.Name, displayName, file.Size, response.MimeType, response.Link, resourceType, subjects ?? [], yearLevels ?? [], topics ?? []);

            var uow = _uowFactory.Create();
            await _userRepository.AddResource(_userId, resource, ct);
            await uow.SaveChangesAsync(ct);

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

        return resourceQuery.Where(r => r.Subjects.Any(s => s.Id == subject.Id)).ToList();
    }
}
