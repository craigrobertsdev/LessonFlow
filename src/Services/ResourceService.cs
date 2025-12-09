using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.Users;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace LessonFlow.Services;

public class ResourceService
{
    private readonly User _user;
    private readonly List<Resource> _resourceCache = [];
    private readonly IStorageManager _storageManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWorkFactory _uowFactory;

    public IReadOnlyList<Resource> ResourceCache => _resourceCache.AsReadOnly();

    private ResourceService(IStorageManager storageManager, IUserRepository userRepository, IUnitOfWorkFactory uowFactory, User user)
    {
        _user = user;
        _storageManager = storageManager;
        _userRepository = userRepository;
        _uowFactory = uowFactory;
    }

    public async Task<Resource?> CreateResource(IBrowserFile file, string displayName, List<Subject> subjects, List<YearLevel> yearLevels, ResourceType resourceType, List<ConceptualOrganiser>? topics, CancellationToken ct)
    {
        var safeFileName = Path.GetRandomFileName();
        try
        {
            var fileStream = file.OpenReadStream(cancellationToken: ct);
            var response = await _storageManager.Save(safeFileName, fileStream, ct);
            var resource = new Resource(_user.Id, file.Name, displayName, file.Size, response.MimeType, response.Link, resourceType, subjects ?? [], yearLevels ?? [], topics ?? []);

            var uow = _uowFactory.Create();
            await _userRepository.AddResource(_user.Id, resource, ct);
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

    public Dictionary<YearLevel, Resource> FilterResources(Subject subject, List<YearLevel>? yearLevels = null)
    {
        var resourceQuery = _resourceCache.AsQueryable();
        if (yearLevels is not null)
        {
            resourceQuery = resourceQuery.Where(r => r.YearLevels.Any(yl => yearLevels.Contains(yl)));
        }

        var resources = resourceQuery.Where(r => r.Subjects.Any(s => s.Id == subject.Id)).ToList();

        Dictionary<YearLevel, Resource> result = [];
        foreach (var resource in resources)
        {
            foreach (var yearLevel in resource.YearLevels)
            {
                if (!_user.CurrentYearPlan!.YearLevelsTaught.Contains(yearLevel)) continue;
                if (!result.TryAdd(yearLevel, resource))
                {
                    result[yearLevel] = resource;
                }
            }
        }

        return result;
    }

    public static async Task<ResourceService> Create(IStorageManager storageManager, IUserRepository userRepository, IUnitOfWorkFactory uowFactory, Guid userId, CancellationToken ct)
    {
        var user = await userRepository.GetById(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        return new ResourceService(storageManager, userRepository, uowFactory, user);
    }
}
