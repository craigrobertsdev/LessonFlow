namespace LessonFlow.Shared.Interfaces.Services;

public interface IStorageManager
{
    Task SaveAsync(string uniqueFileName, Stream fileStream, CancellationToken ct);
    Task SoftDeleteResourceAsync(string uniqueFileName, CancellationToken ct);
    Task HardDeleteAsync(string uniqueFileName, CancellationToken ct);
    Uri GetPresignedUri(string uniqueFileName, bool forceDownload);
    Task<List<string>> GetBlobItems(CancellationToken ct);
}
