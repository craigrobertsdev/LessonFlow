using LessonFlow.Services.FileStorage;

namespace LessonFlow.Shared.Interfaces.Services;

public interface IStorageManager
{
    Task<FileUploadResponse> Save(string fileName, Stream fileStream, CancellationToken ct);
}
