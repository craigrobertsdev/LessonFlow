using LessonFlow.Domain.Resources;

namespace LessonFlow.Services.FileStorage;

public interface IStorageManager
{
    Task<string> Save(string fileName, Stream fileStream, CancellationToken ct);
}
