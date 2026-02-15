using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Persistence;

namespace LessonFlow.Database.Repositories;

public class FileSystemRepository : IFileSystemRepository
{
    public Task<List<FileSystemDirectory>> GetDirectories(FileSystemId fileSystemId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateDirectory(FileSystemDirectory directory)
    {
        throw new NotImplementedException();
    }

    public Task<List<Resource>> GetResources(FileSystemDirectory directory)
    {
        throw new NotImplementedException();
    }
}