using LessonFlow.Database;
using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.IntegrationTests;

internal class TestStorageManager : IStorageManager
{
    private readonly ApplicationDbContext _db;
    private readonly Guid _userId;
    private readonly string EmulatorHostName = "UseDevelopmentStorage=true";

    public TestStorageManager(ApplicationDbContext db, Guid userId)
    {
        _db = db;
        _userId = userId;
    }

    public Task HardDeleteAsync(string uniqueFileName, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetBlobItems(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Uri GetPresignedUri(string uniqueFileName, bool forceDownload)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(string uniqueFileName, Stream fileStream, CancellationToken ct)
    {
        try
        {
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task SoftDeleteResourceAsync(string uniqueFileName, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
