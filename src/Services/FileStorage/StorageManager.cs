using Azure.Storage.Blobs;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.Services.FileStorage;

public class StorageManager : IStorageManager
{
    private readonly BlobContainerClient _blobContainerClient;

    public StorageManager(BlobServiceClient blobClient, AppState appState)
    {
        if (appState.User is null)
        {
            throw new ArgumentException("AppState does not contain a valid User.");
        }

        _blobContainerClient = blobClient.GetBlobContainerClient(appState.User.Id.ToString());
        _blobContainerClient.CreateIfNotExists();
    }

    public async Task SoftDeleteResourceAsync(string uniqueFileName, CancellationToken ct)
    {
        var blob = _blobContainerClient.GetBlobClient(uniqueFileName);
        var deletedBlobName = $"deleted/{uniqueFileName}";
        var deletedBlob = _blobContainerClient.GetBlobClient(deletedBlobName);
        await deletedBlob.StartCopyFromUriAsync(blob.Uri, cancellationToken: ct);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task HardDeleteAsync(string filePath, CancellationToken ct)
    {
        var blobClient = _blobContainerClient.GetBlobClient(filePath);
        await blobClient.DeleteAsync(cancellationToken: ct);
    }

    public Uri GetPresignedUri(string uniqueFileName, bool forceDownload)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(string uniqueFileName, Stream fileStream, CancellationToken ct)
    {
        try
        {
            await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
            var blob = _blobContainerClient.GetBlobClient(uniqueFileName);

            var response = await blob.UploadAsync(fileStream, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<string>> GetBlobItems(CancellationToken ct)
    {
        var names = new List<string>();

        var blobs = _blobContainerClient.GetBlobsAsync(cancellationToken: ct);

        await foreach (var blob in blobs)
        {
            names.Add(blob.Name);
        }

        return names;
    }
}