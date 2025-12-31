using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace LessonFlow.Services;

public class ResourceService
{
    private readonly User _user;
    private readonly Dictionary<Subject, List<Resource>> _resourceCache = [];
    private readonly IStorageManager _storageManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWorkFactory _uowFactory;

    public IReadOnlyDictionary<Subject, List<Resource>> ResourceCache => _resourceCache.AsReadOnly();

    public ResourceService(IStorageManager storageManager, IUserRepository userRepository, IUnitOfWorkFactory uowFactory, AppState appState)
    {
        _storageManager = storageManager;
        _userRepository = userRepository;
        _uowFactory = uowFactory;

        if (appState.User is null)
        {
            throw new ArgumentException("AppState does not contain a valid User.");
        }

        var user = _userRepository.GetWithResources(appState.User.Id, CancellationToken.None).GetAwaiter().GetResult()!;
        appState.User.AddResources(user.Resources);
        _user = user;

        foreach (var resource in user.Resources)
        {
            UpdateResourcCache(resource);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="displayName"></param>
    /// <param name="subjects"></param>
    /// <param name="yearLevels"></param>
    /// <param name="resourceType"></param>
    /// <param name="topics"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws if the file size is greater than the maximum allowable limit or if the upload would exceed the user's storage limit</exception>
    public async Task<Resource?> UploadResourceAsync(IBrowserFile file, string displayName, List<Subject> subjects, List<YearLevel> yearLevels, ResourceType resourceType, List<ConceptualOrganiser>? topics, CancellationToken ct)
    {
        if (file.Size > AppConstants.MAX_RESOURCE_UPLOAD_SIZE_IN_BYTES)
        {
            throw new InvalidOperationException($"File size exceeds the maximum allowable limit of {AppConstants.MAX_RESOURCE_UPLOAD_SIZE_IN_BYTES / 1024 / 1024}MB.");
        }

        if (_user.StorageUsed + file.Size > _user.StorageLimit)
        {
            throw new InvalidOperationException($"User storage limit exceeded.");
        }

        var safeFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
        try
        {
            var fileStream = file.OpenReadStream(cancellationToken: ct);
            await _storageManager.SaveAsync(safeFileName, file.OpenReadStream(AppConstants.MAX_RESOURCE_UPLOAD_SIZE_IN_BYTES, ct), ct);

            var resource = new Resource(_user.Id, file.Name, displayName, file.Size, safeFileName, resourceType, subjects, yearLevels ?? [], topics ?? []);
            _user.StorageUsed += file.Size;

            await using var uow = _uowFactory.Create();
            await _userRepository.AddResourceAsync(_user, resource, ct);
            await uow.SaveChangesAsync(ct);

            UpdateResourcCache(resource);
            return resource;
        }
        catch (Exception e)
        {
            await _storageManager.HardDeleteAsync(safeFileName, CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    private void UpdateResourcCache(Resource resource)
    {
        foreach (var subject in resource.Subjects)
        {
            if (!_resourceCache.TryGetValue(subject, out var _))
            {
                _resourceCache.Add(subject, [resource]);
            }
            else
            {
                _resourceCache[subject].Add(resource);
            }
        }
    }

    public async Task SoftDeleteResourceAsync(Resource resource, CancellationToken ct)
    {
        if (resource.IsSoftDeleted)
        {
            throw new InvalidOperationException("Resource is already marked for deletion.");
        }

        foreach (var subject in resource.Subjects)
        {
            _resourceCache[subject].Remove(resource);
        }

        await using var uow = _uowFactory.Create();
        await _storageManager.SoftDeleteResourceAsync(resource.FileName, ct);
        await _userRepository.SoftDeleteResourceAsync(resource, ct);
        await uow.SaveChangesAsync(ct);
    }

    public Dictionary<YearLevel, List<Resource>> FilterResources(Subject subject, List<YearLevel>? yearLevels = null)
    {
        if (!_resourceCache.TryGetValue(subject, out var cachedResources))
        {
            return [];
        }

        var resourceQuery = cachedResources.AsQueryable();
        if (yearLevels is not null)
        {
            resourceQuery = resourceQuery.Where(r => r.YearLevels.Any(yl => yearLevels.Contains(yl)));
        }

        var resources = resourceQuery.Where(r => r.Subjects.Any(s => s.Id == subject.Id)).ToList();

        Dictionary<YearLevel, List<Resource>> result = [];
        foreach (var resource in resources)
        {
            foreach (var yearLevel in resource.YearLevels)
            {
                if (!_user.CurrentYearPlan!.YearLevelsTaught.Contains(yearLevel) || (yearLevels is not null && !yearLevels.Contains(yearLevel))) continue;
                if (!result.TryGetValue(yearLevel, out List<Resource>? value))
                {
                    result.Add(yearLevel, [resource]);
                }
                else
                {
                    result[yearLevel].Add(resource);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a list of all resources belonging to the current user for the current subject
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<List<Resource>> GetResourceMetadataAsync(Subject subject, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Resource>> GetResourceUrlAsync(ResourceId resourceId, bool forceDownload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateResourceMetadataAsync(ResourceId resourceId, string newDisplayName, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Resource>> ShowResourcesPendingDeletion(CancellationToken ct)
    {
        return await _userRepository.GetSoftDeletedResourcesAsync(_user.Id, ct);
    }

    public async Task RecoverDeletedResourceAsync(ResourceId resourceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task HardDeleteResourcesAsync(CancellationToken ct)
    {
        try
        {
            var resourcesPendingDeletion = await _userRepository.GetResourcesDueForDeletionAsync(ct);
            List<Task> fileDeleteTasks = [];
            foreach (var resource in resourcesPendingDeletion)
            {
                fileDeleteTasks.Add(_storageManager.HardDeleteAsync(resource.Link, ct));
            }

            await using var uow = _uowFactory.Create();
            var dbDeleteTask = _userRepository.HardDeleteResourcesAsync(resourcesPendingDeletion.Select(r => r.Id), ct);
            await Task.WhenAll([.. fileDeleteTasks, dbDeleteTask]);
            await uow.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
