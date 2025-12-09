using LessonFlow.Domain.Resources;
using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.Services.FileStorage;

public class StorageManager : IStorageManager
{
    private readonly Guid _userId;
    private const string BASE_PATH = "unsafe_uploads";
    private string directoryPath => Path.Combine(BASE_PATH, _userId.ToString());

    public StorageManager()
    {
        _userId = Guid.NewGuid();
    }
    public async Task<string> Save(string fileName, Stream file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}