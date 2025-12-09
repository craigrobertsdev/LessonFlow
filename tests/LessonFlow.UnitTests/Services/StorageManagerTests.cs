using LessonFlow.Services.FileStorage;

namespace LessonFlow.UnitTests.Services;
public class StorageManagerTests
{
    [Fact]
    public async Task CreateResource_WhenProvidedFile_ReturnsPathToFile()
    {
        var userId = Guid.NewGuid();
        var filePath = Environment.CurrentDirectory + userId.ToString();
        var storageManager = new StorageManager();

        using var stream = new MemoryStream(16000);
        var resultUrl = await storageManager.UploadResource(stream, CancellationToken.None);

        Assert.Equal(filePath, resultUrl);
    }

    [Fact]
    public async Task CreateResource_WhenProvidedFile_StoresFileInUserDirectory()
    {
        var userId = Guid.NewGuid();
        var filePath = Environment.CurrentDirectory + userId.ToString();
        var storageManager = new StorageManager();
        using var stream = new MemoryStream(16000);
        var resultUrl = await storageManager.UploadResource(stream, CancellationToken.None);
        var fileExists = File.Exists(resultUrl);
        Assert.True(fileExists);

        // Clean up
        if (fileExists)
        {
            File.Delete(resultUrl);
            Directory.Delete(filePath);
        }
    }
}
