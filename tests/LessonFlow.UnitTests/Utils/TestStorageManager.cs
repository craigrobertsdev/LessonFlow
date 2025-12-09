using LessonFlow.Database;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared.Interfaces.Services;

namespace LessonFlow.UnitTests.Utils;

internal class TestStorageManager : IStorageManager
{
    private readonly ApplicationDbContext _db;
    private readonly Guid _userId;

    public TestStorageManager(ApplicationDbContext db, Guid userId)
    {
        _db = db;
        _userId = userId;
    }

    public Task<FileUploadResponse> Save(string fileName, Stream fileStream, CancellationToken ct)
    {
        var fileLink = $"test-{fileName}";
        var response = new FileUploadResponse("application/pdf", fileLink);
        return Task.FromResult(response);
    }
}
