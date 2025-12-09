using LessonFlow.Database;
using LessonFlow.Database.Repositories;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.Users;
using LessonFlow.Services;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.UnitTests.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;

namespace LessonFlow.UnitTests.Services;

public class ResourceServiceTests
{
    private readonly User _user;
    private readonly ApplicationDbContext _db;
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly string _resourceName;
    private readonly ResourceType _resourceType;
    private readonly List<Subject> _subjects;
    private readonly List<YearLevelValue> _yearLevels;

    public ResourceServiceTests()
    {
        _user = new User();
        _db = GetTestDb();
        _resourceName = "Sample Resource";
        _resourceType = ResourceType.Video;
        _subjects = [new Subject([], "Mathematics")];
        _yearLevels = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };

    }

    [Fact]
    public async Task CreateResource_WhenCalledWithValidParameters_ShouldCreateResource()
    {

        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.NotNull(resource);
        Assert.Equal(_resourceName, resource.DisplayName);
        Assert.Equal(_resourceType, resource.Type);
        Assert.NotEqual(string.Empty, resource.Link);
        Assert.Equal(_user.Id, resource.UserId);
        Assert.Equal(_yearLevels, resource.YearLevels);
    }

    [Fact]
    public void ResourceCache_WhenNoResourcesCreated_ShouldBeEmpty()
    {

        var resourceService = GetTestResourceService();
        Assert.Empty(resourceService.ResourceCache);
    }

    [Fact]
    public async Task CreateResource_WhenCalled_CreatedResourceAddedToResourceCache()
    {

        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.Single(resourceService.ResourceCache);
        Assert.Contains(resource, resourceService.ResourceCache);
    }

    [Fact]
    public async Task CreateResource_WhenCalledWithNullStrands_ShouldCreateResourceWithEmptyStrands()
    {

        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        Assert.NotNull(resource);
        Assert.Empty(resource.ConceptualOrganisers);
    }

    [Fact]
    public async Task CreateResource_WhenCalledWithStrands_ShouldCreateResourceWithGivenStrands()
    {

        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();

        var topics = new List<ConceptualOrganiser> { new ConceptualOrganiser() { Name = "Algebra" }, new ConceptualOrganiser() { Name = "Number" } };
        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, topics, new CancellationToken());
        Assert.Equal(topics, resource.ConceptualOrganisers);
    }

    [Fact]
    public async Task CreateResource_WhenAddingTwoResourcesWithSameName_ShouldCreateDistinctResources()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var resource1 = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        var resource2 = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        Assert.NotNull(resource1);
        Assert.NotNull(resource2);
        Assert.NotEqual(resource1.Id, resource2.Id);
        Assert.Equal(2, resourceService.ResourceCache.Count);
    }

    [Fact]
    public async Task DeleteResource_WhenResourceExists_ShouldRemoveFromCache()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        resourceService.DeleteResource(resource!);
        Assert.DoesNotContain(resource, resourceService.ResourceCache);
    }

    [Fact]
    public async Task DeleteResource_WhenTwoResourcesWithSameNameExist_ShouldRemoveOnlySpecifiedResource()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var resource1 = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        var resource2 = await resourceService.CreateResource(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.NotNull(resource1);
        resourceService.DeleteResource(resource1);
        Assert.DoesNotContain(resource1, resourceService.ResourceCache);
        Assert.Contains(resource2, resourceService.ResourceCache);
    }

    [Fact]
    public async Task FilterResources_WhenSubjectIsSpecifiedAndResourcesExist_ShouldReturnOnlyResourcesOfThatSubject()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var subjectMath = new Subject([], "Mathematics");
        var subjectScience = new Subject([], "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.CreateResource(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Science Resource", [subjectScience], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectMath);
        Assert.Single(filteredResources);
        Assert.Contains(resource1, filteredResources);
        Assert.DoesNotContain(resource2, filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenNoResourcesOfSubjectExist_ShouldReturnEmptyList()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var subjectMath = new Subject([], "Mathematics");
        var subjectScience = new Subject([], "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.CreateResource(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectScience);
        Assert.Empty(filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenMultipleResourcesOfSubjectExist_ShouldReturnAllResourcesOfThatSubject()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var subjectMath = new Subject([], "Mathematics");
        var subjectScience = new Subject([], "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.CreateResource(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Science Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource3 = await resourceService.CreateResource(file.Object, "Science Resource", [subjectScience], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectMath);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year2, YearLevelValue.Year3 };
        var subject = new Subject([], "Mathematics");
        var resource1 = await resourceService.CreateResource(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);

        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Reception]);

        Assert.Single(filteredResources);
        Assert.Contains(resource1, filteredResources);
        Assert.DoesNotContain(resource2, filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenNoResourcesInCacheExistForSpecifiedYearLevel_ReturnEmptyList()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year2, YearLevelValue.Year3 };
        var subject = new Subject([], "Mathematics");
        var resource1 = await resourceService.CreateResource(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);

        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year5]);

        Assert.Empty(filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenMultipleResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year1, YearLevelValue.Year2 };
        var subject = new Subject([], "Mathematics");
        var resource1 = await resourceService.CreateResource(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);
        var resource3 = await resourceService.CreateResource(file.Object, "Resource 3", [subject], [YearLevelValue.Year3], ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year1]);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
        Assert.DoesNotContain(resource3, filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenResourcesExistForSpecifiedConceptualOrganisers_ReturnThoseResources()
    {
        var resourceService = GetTestResourceService();
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year1, YearLevelValue.Year2 };
        var subject = new Subject([], "Mathematics");
        var resource1 = await resourceService.CreateResource(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.CreateResource(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);
        var resource3 = await resourceService.CreateResource(file.Object, "Resource 3", [subject], [YearLevelValue.Year3], ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year1]);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
        Assert.DoesNotContain(resource3, filteredResources);
    }

    [Fact]
    public async Task UploadResource_WhenFileUploadedAndMetadataProvided_ShouldUploadFileAndPersistAndMetadata()
    {
        var resourceService = GetTestResourceService();
        var user = _db.Users.First();
        var subject = _db.Subjects.First();
        var file = new Mock<IBrowserFile>();
        file.Setup(f => f.Name).Returns("testfile.pdf");
        file.Setup(f => f.Size).Returns(1024);
        file.Setup(f => f.ContentType).Returns("application/pdf");

        var resource = await resourceService.CreateResource(file.Object, "Test Resource", [subject], [YearLevelValue.Year1], ResourceType.Worksheet, [], new CancellationToken());

        Assert.NotNull(resource);
        Assert.Equal("testfile.pdf", resource.FileName);
        Assert.Equal("Test Resource", resource.DisplayName);
        Assert.Equal(1024, resource.FileSize);
        Assert.Equal("application/pdf", resource.MimeType);
        Assert.Equal(user.Id, resource.UserId);
        Assert.NotEqual(string.Empty, resource.Link);
        Assert.Single(resource.Subjects);
        Assert.Equal(subject.Id, resource.Subjects.First().Id);
        Assert.Single(resource.YearLevels);
        Assert.Equal(YearLevelValue.Year1, resource.YearLevels.First());

        var storedResource = _db.Resources.FirstOrDefault(r => r.Id == resource.Id);
        Assert.NotNull(storedResource);
    }

    private ApplicationDbContext GetTestDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;
        var db = new ApplicationDbContext(options);
        var subject = new Subject([], "Mathematics");
        var yearLevel = new YearLevel(YearLevelValue.Year1, "Learning Standard");
        subject.AddYearLevel(yearLevel);
        db.Subjects.Add(subject);
        db.Users.Add(_user);
        db.SaveChanges();

        return db;
    }

    private ResourceService GetTestResourceService()
    {
        var storageManager = new TestStorageManager(_db, _user.Id);

        var dbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        dbContextFactory.Setup(f => f.CreateDbContext()).Returns(_db);
        var ambient = new Mock<IAmbientDbContextAccessor<ApplicationDbContext>>();
        ambient.Setup(a => a.Current).Returns(_db);
        var uowFactory = new UnitOfWorkFactory<ApplicationDbContext>(dbContextFactory.Object, ambient.Object);
        var userRepository = new UserRepository(dbContextFactory.Object, ambient.Object);

        var resourceService = new ResourceService(storageManager, userRepository, uowFactory, _user.Id);

        return resourceService;
    }
}
