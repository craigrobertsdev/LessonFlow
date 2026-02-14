using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IResourceRepository
{
    Task<List<ResourceDirectory>> GetDirectories(FileSystemId fileSystemId);
}