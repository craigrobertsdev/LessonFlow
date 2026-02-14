using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Database.Repositories;

public class ResourceRepository : IResourceRepository
{
    public Task<List<ResourceDirectory?>> GetDirectories(FileSystemId fileSystemId)
    {
        throw new NotImplementedException();
    }
}