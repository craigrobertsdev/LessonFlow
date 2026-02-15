using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IFileSystemRepository
{
    Task<List<FileSystemDirectory>> GetDirectories(FileSystemId fileSystemId);
    Task UpdateDirectory(FileSystemDirectory directory);
    Task<List<Resource>> GetResources(FileSystemDirectory directory);
}