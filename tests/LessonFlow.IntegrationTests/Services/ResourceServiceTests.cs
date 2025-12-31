using Azure.Storage.Blobs;
using LessonFlow.Database;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Services;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

namespace LessonFlow.IntegrationTests.Services;

public class ResourceServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly ApplicationDbContext _dbContext;

    public ResourceServiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        SeedDbContext(_dbContext);
    }

    [Fact]
    public async Task UploadResource_WhenFileUploadedAndMetadataProvided_ShouldUploadFileAndPersistAndMetadata()
    {
        try
        {
            var resourceService = await CreateResourceService();
            var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
            var subject = _dbContext.Subjects.First();

            var fileName = "testfile.pdf";
            var fileSize = 1024;
            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.ContentType).Returns("application/pdf");
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);

            var resource = await resourceService.UploadResourceAsync(file.Object, "Test Resource", [subject], [YearLevel.Year1], ResourceType.Worksheet, [], new CancellationToken());

            Assert.NotNull(resource);
            Assert.Equal(fileName, resource.FileName);
            Assert.Equal("Test Resource", resource.DisplayName);
            Assert.Equal(fileSize, resource.FileSize);
            Assert.Equal(user.Id, resource.UserId);
            Assert.NotEqual(string.Empty, resource.Link);
            Assert.Single(resource.Subjects);
            Assert.Equal(subject.Id, resource.Subjects.First().Id);
            Assert.Single(resource.YearLevels);
            Assert.Equal(YearLevel.Year1, resource.YearLevels.First());
            Assert.Equal(1024, resource.FileSize);
            Assert.True(DateTime.UtcNow - resource.CreatedDateTime < TimeSpan.FromSeconds(1));

            var containerClient = new BlobServiceClient("UseDevelopmentStorage=true").GetBlobContainerClient(user.Id.ToString());
            var blobClient = containerClient.GetBlobClient(resource.Link);
            Assert.NotNull(blobClient);
            var blob = await blobClient.DownloadContentAsync();
            Assert.NotNull(blob);

            var downloadedContent = blob.Value.Content.ToArray();
            Assert.Equal(buffer, downloadedContent);

            var storedResource = _dbContext.Resources.FirstOrDefault(r => r.Id == resource.Id);
            Assert.NotNull(storedResource);
        }
        finally
        {
            var containerClient = new BlobServiceClient("UseDevelopmentStorage=true").GetBlobContainerClient(_dbContext.Users.First(u => u.Email == TestUserEmail).Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task UploadResource_WhenFileSizeExceedsMaximum_ShouldReturnError()
    {
        try
        {
            var resourceService = await CreateResourceService();
            var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
            var subject = _dbContext.Subjects.First();

            var fileName = "largefile.pdf";
            var fileSize = AppConstants.MAX_RESOURCE_UPLOAD_SIZE_IN_BYTES + 1;
            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.ContentType).Returns("application/pdf");
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);

            var act = async () => await resourceService.UploadResourceAsync(file.Object, "Test Resource", [subject], [YearLevel.Year1], ResourceType.Worksheet, [], new CancellationToken());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("File size exceeds the maximum allowable limit of 50MB.", ex.Message);
        }
        finally
        {
            var containerClient = new BlobServiceClient("UseDevelopmentStorage=true").GetBlobContainerClient(_dbContext.Users.First(u => u.Email == TestUserEmail).Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task UploadResource_WhenFileExceedsUserStorageLimit_ShouldReturnError()
    {
        try
        {
            var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
            user.StorageUsed = user.StorageLimit - 1024;
            await _dbContext.SaveChangesAsync();

            var resourceService = await CreateResourceService();

            var subject = _dbContext.Subjects.First();
            var fileName = "testfile.pdf";
            var fileSize = 2048;
            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.ContentType).Returns("application/pdf");
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);
            var act = async () => await resourceService.UploadResourceAsync(file.Object, "Test Resource", [subject], [YearLevel.Year1], ResourceType.Worksheet, [], new CancellationToken());
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("User storage limit exceeded.", ex.Message);
        }
        finally
        {
            var containerClient = new BlobServiceClient("UseDevelopmentStorage=true").GetBlobContainerClient(_dbContext.Users.First(u => u.Email == TestUserEmail).Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task UploadResource_WhenResourceUploaded_ShouldCreateStorageBucketForIndividualUser()
    {
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        var client = new BlobServiceClient("UseDevelopmentStorage=true");
        var containerClient = client.GetBlobContainerClient(user.Id.ToString());

        try
        {
            var fileName = "testfile.pdf";
            var fileSize = 1024;
            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.ContentType).Returns("application/pdf");
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);

            var serviceCollection = _factory.Services;
            var blobServiceClient = serviceCollection.GetRequiredService<BlobServiceClient>();
            var storageManager = new StorageManager(blobServiceClient, await CreateAppState());
            await storageManager.SaveAsync(file.Object.Name, file.Object.OpenReadStream(), new CancellationToken());

            Assert.NotNull(containerClient);

            var blobClient = containerClient.GetBlobClient(fileName);
            Assert.NotNull(blobClient);
            var blob = await blobClient.DownloadContentAsync();
            Assert.NotNull(blob);

            var downloadedContent = blob.Value.Content.ToArray();
            Assert.Equal(buffer, downloadedContent);
        }
        finally
        {
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteResource_WhenResourceDeleted_ShouldSoftDeleteForAppropriateRetentionPeriod()
    {
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        try
        {
            var subject = _dbContext.Subjects.First();
            var file = new Mock<IBrowserFile>();
            var fileName = "testfile1.pdf";
            var fileSize = 1024;
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);

            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());

            containerClient.CreateIfNotExists();
            _dbContext.Resources.Add(new Resource(user.Id, file.Object.Name, "Test Resource", file.Object.Size, $"{user.Id}/{file.Object.Name}", ResourceType.Worksheet, [subject], [YearLevel.Year1], []));
            _dbContext.SaveChanges();
            containerClient.UploadBlob(file.Object.Name, file.Object.OpenReadStream());

            var resourceService = await CreateResourceService();
            var resource = resourceService.ResourceCache[subject].First();

            await resourceService.SoftDeleteResourceAsync(resource, new CancellationToken());


            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedResource = dbContext.Resources.First(r => r.Id == resource.Id);
            Assert.True(deletedResource.IsSoftDeleted);
            Assert.Equal(DateTime.UtcNow.AddDays(AppConstants.SOFT_DELETION_PERIOD_DAYS).Date, deletedResource.DeletionDate.Date);

            var blobs = containerClient.GetBlobs().ToList();
            Assert.Single(blobs);
            Assert.Equal($"deleted/{fileName}", blobs[0].Name);
        }
        finally
        {
            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteResource_WhenResourceDoesNotExist_ShouldThrowException()
    {
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        try
        {
            var subject = _dbContext.Subjects.First();
            var file = new Mock<IBrowserFile>();
            var fileName = "testfile1.pdf";
            var fileSize = 1024;
            file.Setup(f => f.Name).Returns(fileName);
            file.Setup(f => f.Size).Returns(fileSize);
            var buffer = new byte[fileSize];
            new Random().NextBytes(buffer);
            var stream = new MemoryStream(buffer);
            file.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);

            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());

            containerClient.CreateIfNotExists();
            _dbContext.Resources.Add(new Resource(user.Id, file.Object.Name, "Test Resource", file.Object.Size, $"{user.Id}/{file.Object.Name}", ResourceType.Worksheet, [subject], [YearLevel.Year1], []));
            _dbContext.SaveChanges();
            containerClient.UploadBlob(file.Object.Name, file.Object.OpenReadStream());

            var resourceService = await CreateResourceService();
            var resource = resourceService.ResourceCache[subject].First();
            await resourceService.SoftDeleteResourceAsync(resource, new CancellationToken());
            resource.MarkAsDeleted();

            var act = async () => await resourceService.SoftDeleteResourceAsync(resource, new CancellationToken());
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        }
        finally
        {
            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task ShowResourcesPendingDeletion_WhenCalled_ShouldReturnAllResourcesForUserPendingDeletion()
    {
        try
        {
            var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
            var subject = _dbContext.Subjects.First();
            var resource1 = new Resource(user.Id, "file1.pdf", "Resource 1", 1024, "file1.pdf", ResourceType.Worksheet, [subject], [YearLevel.Year1], []);
            resource1.MarkAsDeleted();
            var resource2 = new Resource(user.Id, "file2.pdf", "Resource 2", 2048, "file2.pdf", ResourceType.Worksheet, [subject], [YearLevel.Year1], []);
            resource2.MarkAsDeleted();
            _dbContext.Resources.AddRange([resource1, resource2]);
            await _dbContext.SaveChangesAsync();
            var resourceService = await CreateResourceService();

            var pendingDeletionResources = await resourceService.ShowResourcesPendingDeletion(new CancellationToken());

            Assert.Equal(2, pendingDeletionResources.Count);
            Assert.Contains(pendingDeletionResources, r => r.Id == resource1.Id);
            Assert.Contains(pendingDeletionResources, r => r.Id == resource2.Id);
        }
        finally
        {
            var containerClient = new BlobServiceClient("UseDevelopmentStorage=true").GetBlobContainerClient(_dbContext.Users.First(u => u.Email == TestUserEmail).Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task HardDeleteResourcesAsync_WhenResourceDeletionDateReached_ShouldDeleteSingleResourcePermanently()
    {
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        try
        {
            var subject = _dbContext.Subjects.First();
            var resource = new Resource(user.Id, "file1.pdf", "Resource 1", 1024, "file1.pdf", ResourceType.Worksheet, [subject], [YearLevel.Year1], []);
            resource.MarkAsDeleted();
            resource.GetType().GetProperty("DeletionDate")!.SetValue(resource, DateTime.UtcNow.AddDays(-1));
            _dbContext.Resources.Add(resource);
            _dbContext.SaveChanges();

            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            containerClient.CreateIfNotExists();
            containerClient.UploadBlob("file1.pdf", new MemoryStream(new byte[1024]));
            var resourceService = await CreateResourceService();

            await resourceService.HardDeleteResourcesAsync(new CancellationToken());

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedResource = context.Resources.Find(resource.Id);
            Assert.Null(deletedResource);
            var blobs = containerClient.GetBlobs().ToList();
            Assert.Empty(blobs);
        }
        finally
        {
            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    [Fact]
    public async Task HardDeleteResourcesAsync_WhenResourceDeletionDateReached_ShouldDeleteMultipleResourcesPermanently()
    {
        var user = _dbContext.Users.First(u => u.Email == TestUserEmail);
        try
        {
            var subject = _dbContext.Subjects.First();
            List<Resource> resources =
            [
                new Resource(user.Id, "file1.pdf", "Resource 1", 1024, "file1.pdf", ResourceType.Worksheet, [subject], [YearLevel.Year1], []),
                new Resource(user.Id, "file2.pdf", "Resource 2", 1024, "file2.pdf", ResourceType.Worksheet, [subject], [YearLevel.Year1], []),
            ];

            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            containerClient.CreateIfNotExists();

            foreach (var resource in resources)
            {
                resource.MarkAsDeleted();
                resource.GetType().GetProperty("DeletionDate")!.SetValue(resource, DateTime.UtcNow.AddDays(-1));
                _dbContext.Resources.Add(resource);
                _dbContext.SaveChanges();
                containerClient.UploadBlob(resource.FileName, new MemoryStream(new byte[1024]));
            }

            var resourceService = await CreateResourceService();
            await resourceService.HardDeleteResourcesAsync(new CancellationToken());

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var deletedResources = new List<Resource>();
            foreach (var resource in resources)
            {
                var deletedResource = context.Resources.Find(resource.Id);
                if (deletedResource is not null) deletedResources.Add(deletedResource);
            }
            Assert.Empty(deletedResources);

            var blobs = containerClient.GetBlobs().ToList();
            Assert.Empty(blobs);
        }
        finally
        {
            var containerClient = _factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(user.Id.ToString());
            await containerClient.DeleteAsync();
        }
    }

    private async Task<ResourceService> CreateResourceService()
    {
        var appState = await CreateAppState();
        var scope = _factory.Services.CreateScope();
        var uowFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var storageManager = new StorageManager(scope.ServiceProvider.GetRequiredService<BlobServiceClient>(), await CreateAppState());
        var resourceService = new ResourceService(storageManager, userRepository, uowFactory, appState);

        return resourceService;
    }

    private async Task<AppState> CreateAppState()
    {
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppState>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByEmail(TestUserEmail, default);
        var authStateProvider = new Mock<AuthenticationStateProvider>();
        authStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, user!.Email!)]))));
        var termDatesService = new Mock<ITermDatesService>().Object;
        var appState = new AppState(authStateProvider.Object, userRepository, logger, termDatesService);
        appState.User = user;
        await appState.InitialiseAsync();

        return appState;
    }
}
