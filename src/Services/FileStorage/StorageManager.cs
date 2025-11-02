using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.Services.FileStorage;

public class StorageManager : IStorageManager
{
    public async Task<string> UploadResource(Stream file, CancellationToken ct)
    {
        return await Task.FromResult("https://google.com.au");
    }
}