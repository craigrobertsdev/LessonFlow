using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Database;
using LessonFlow.Database.Repositories;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Services;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using LessonFlow.UnitTests.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using static LessonFlow.UnitTests.UnitTestHelpers;

namespace LessonFlow.UnitTests.Services;

public class ResourceServiceTests : IDisposable
{
    private readonly User _user;
    private readonly string _resourceName;
    private readonly ResourceType _resourceType;
    private readonly List<Subject> _subjects;
    private readonly List<YearLevel> _yearLevels;

    private readonly SqliteConnection _connection;

    public ResourceServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _user = new User() { Id = Guid.NewGuid() };
        _resourceName = "Sample Resource";
        _resourceType = ResourceType.Video;
        _subjects = [
            new Subject([], "Mathematics"),
            new Subject([], "Science")
        ];
        _yearLevels = [YearLevel.Reception, YearLevel.Year1];
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    [Fact]
    public async Task Initialise_WhenMultipleUsersExist_ShouldOnlyLoadCurrentUserResources()
    {
        var db = GetTestDb();
        var user2 = new User()
        {
            Email = "seconduser@test.com"
        };
        db.Users.Add(user2);

        var subject = _subjects.First();

        var user2Resource = new Resource(user2.Id, "user2File", "user2File", 1024, "testLink", ResourceType.Worksheet, [subject], [YearLevel.Year1]);
        user2.Resources.Add(user2Resource);
        db.Resources.Add(user2Resource);
        db.SaveChanges();

        var user1Resource = new Resource(_user.Id, "user1File", "user1File", 1024, "testLink", ResourceType.Worksheet, [subject], [YearLevel.Year1]);

        var user1 = db.Users
            .Include(u => u.Resources)
            .First(u => u.Id == _user.Id);
        user1.AddResource(user1Resource);
        db.SaveChanges();

        var resourceService = await GetTestResourceService(db);

        Assert.True(resourceService.ResourceCache.ContainsKey(subject));
        Assert.Single(resourceService.ResourceCache[subject]);
        Assert.Equal(resourceService.ResourceCache[subject].First().UserId, _user.Id);
        Assert.Equal(resourceService.ResourceCache[subject].First().Id, user1Resource.Id);
    }

    [Fact]
    public async Task CreateResource_WhenCalledWithValidParameters_ShouldCreateResource()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.NotNull(resource);
        Assert.Equal(_resourceName, resource.DisplayName);
        Assert.Equal(_resourceType, resource.Type);
        Assert.NotEqual(string.Empty, resource.Link);
        Assert.Equal(_user.Id, resource.UserId);
        Assert.Equal(_yearLevels, resource.YearLevels);
    }

    [Fact]
    public async Task ResourceCache_WhenNoResourcesCreated_ShouldBeEmpty()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        Assert.Empty(resourceService.ResourceCache);
    }

    [Fact]
    public async Task CreateResource_WhenCalled_CreatedResourceAddedToResourceCache()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.Single(resourceService.ResourceCache);
        Assert.Contains(resource, resourceService.ResourceCache[_subjects.First()]);
    }

    [Fact]
    public async Task CreateResource_WhenCalledWithNullStrands_ShouldCreateResourceWithEmptyStrands()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        Assert.NotNull(resource);
        Assert.Empty(resource.ConceptualOrganisers);
    }

    [Fact]
    public async Task CreateResource_WhenCalledWithStrands_ShouldCreateResourceWithGivenStrands()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();

        var topics = new List<ConceptualOrganiser> { new() { Name = "Algebra" }, new() { Name = "Number" } };
        var resource = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, topics, new CancellationToken());
        Assert.NotNull(resource);
        Assert.Equal(topics, resource.ConceptualOrganisers);
    }

    [Fact]
    public async Task CreateResource_WhenAddingTwoResourcesWithSameName_ShouldCreateDistinctResources()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var resource1 = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        var resource2 = await resourceService.UploadResourceAsync(file.Object, _resourceName, _subjects, _yearLevels, _resourceType, [], new CancellationToken());
        Assert.NotNull(resource1);
        Assert.NotNull(resource2);
        Assert.NotEqual(resource1.Id, resource2.Id);
        Assert.Equal(2, resourceService.ResourceCache[resource1.Subjects[0]].Count);
    }

    [Fact]
    public async Task DeleteResource_WhenResourceExists_ShouldRemoveFromCache()
    {
        Resource resource;
        using (var db = GetTestDb())
        {
            var resourceService = await GetTestResourceService(db);
            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.Name).Returns("TestResource");
            resource = new Resource(_user.Id, file.Name, "TestResource", 1024, "TestResource", ResourceType.Assessment, _subjects, [], []);
            db.Resources.Add(resource);
            db.SaveChanges();
        }

        using (var db = CreateContext())
        {
            var resourceService = await GetTestResourceService(db);
            await resourceService.SoftDeleteResourceAsync(resource!, new CancellationToken());

            Assert.DoesNotContain(resource, resourceService.ResourceCache[_subjects.First()]);
        }
    }

    [Fact]
    public async Task DeleteResource_WhenTwoResourcesWithSameNameExist_ShouldRemoveOnlySpecifiedResource()
    {
        Resource resource1, resource2;
        using (var db = GetTestDb())
        {
            var resourceService = await GetTestResourceService(db);
            var file = new Mock<IBrowserFile>();
            resource1 = new Resource(_user.Id, file.Name, "TestResource", 1024, "TestResource", ResourceType.Assessment, _subjects, [], []);
            resource2 = new Resource(_user.Id, file.Name, "TestResource", 1024, "TestResource", ResourceType.Assessment, _subjects, [], []);
            db.Resources.AddRange(resource1, resource2);
            db.SaveChanges();
        }

        using (var db = CreateContext())
        {
            var resourceService = await GetTestResourceService(db);
            await resourceService.SoftDeleteResourceAsync(resource1, new CancellationToken());
            Assert.DoesNotContain(resource1, resourceService.ResourceCache[_subjects.First()]);
            Assert.Contains(resource2, resourceService.ResourceCache[_subjects.First()]);
        }
    }

    [Fact]
    public async Task FilterResources_WhenSubjectIsSpecifiedAndResourcesExist_ShouldReturnOnlyResourcesOfThatSubject()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var subjectMath = _subjects.First(s => s.Name == "Mathematics");
        var subjectScience = _subjects.First(s => s.Name == "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Science Resource", [subjectScience], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectMath);

        List<Resource> flattenedResources = filteredResources.SelectMany(k => k.Value).ToList();
        Assert.Single(flattenedResources);
        Assert.Contains(resource1, flattenedResources);
        Assert.DoesNotContain(resource2, flattenedResources);
    }

    [Fact]
    public async Task FilterResources_WhenNoResourcesOfSubjectExist_ShouldReturnEmptyList()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var subjectMath = _subjects.First(s => s.Name == "Mathematics");
        var subjectScience = _subjects.First(s => s.Name == "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectScience);
        Assert.Empty(filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenMultipleResourcesOfSubjectExist_ShouldReturnAllResourcesOfThatSubject()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var subjectMath = _subjects.First(s => s.Name == "Mathematics");
        var subjectScience = _subjects.First(s => s.Name == "Science");
        var ct = new CancellationToken();
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Math Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Science Resource", [subjectMath], _yearLevels, ResourceType.Video, [], ct);
        var resource3 = await resourceService.UploadResourceAsync(file.Object, "Science Resource", [subjectScience], _yearLevels, ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subjectMath);

        List<Resource> flattenedResources = filteredResources.SelectMany(k => k.Value).ToList();
        Assert.Equal(2, flattenedResources.Count);
        Assert.Contains(resource1, flattenedResources);
        Assert.Contains(resource2, flattenedResources);
    }

    [Fact]
    public async Task FilterResources_WhenResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevel> { YearLevel.Reception, YearLevel.Year1 };
        var yearLevels2 = new List<YearLevel> { YearLevel.Year2, YearLevel.Year3 };
        var subject = _subjects.First(s => s.Name == "Mathematics");
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);

        var filteredResources = resourceService.FilterResources(subject, [YearLevel.Year2]);

        List<Resource> flattenedResources = filteredResources.SelectMany(k => k.Value).ToList();
        Assert.Single(flattenedResources);
        Assert.Contains(resource2, flattenedResources);
        Assert.DoesNotContain(resource1, flattenedResources);
    }

    [Fact]
    public async Task FilterResources_WhenNoResourcesInCacheExistForSpecifiedYearLevel_ReturnEmptyList()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevel> { YearLevel.Reception, YearLevel.Year1 };
        var yearLevels2 = new List<YearLevel> { YearLevel.Year2, YearLevel.Year3 };
        var subject = _subjects.First(s => s.Name == "Mathematics");
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);

        var filteredResources = resourceService.FilterResources(subject, [YearLevel.Year5]);

        Assert.Empty(filteredResources);
    }

    [Fact]
    public async Task FilterResources_WhenMultipleResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevel> { YearLevel.Reception, YearLevel.Year1 };
        var yearLevels2 = new List<YearLevel> { YearLevel.Year1, YearLevel.Year2 };
        var subject = _subjects.First();
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);
        var resource3 = await resourceService.UploadResourceAsync(file.Object, "Resource 3", [subject], [YearLevel.Year3], ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subject, [YearLevel.Year1]);

        List<Resource> flattenedResources = filteredResources.SelectMany(k => k.Value).ToList();
        Assert.Equal(2, flattenedResources.Count);
        Assert.Contains(resource1, flattenedResources);
        Assert.Contains(resource2, flattenedResources);
        Assert.DoesNotContain(resource3, flattenedResources);
    }

    [Fact]
    public async Task FilterResources_WhenResourcesExistForSpecifiedConceptualOrganisers_ReturnThoseResources()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevel> { YearLevel.Reception, YearLevel.Year1 };
        var yearLevels2 = new List<YearLevel> { YearLevel.Year1, YearLevel.Year2 };
        var subject = _subjects.First(s => s.Name == "Mathematics");
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);
        var resource3 = await resourceService.UploadResourceAsync(file.Object, "Resource 3", [subject], [YearLevel.Year3], ResourceType.Video, [], ct);
        var filteredResources = resourceService.FilterResources(subject, [YearLevel.Year1]);

        var flattenedResources = filteredResources.SelectMany((k) => k.Value).ToList();
        Assert.Equal(2, flattenedResources.Count);
        Assert.Contains(resource1, flattenedResources);
        Assert.Contains(resource2, flattenedResources);
        Assert.DoesNotContain(resource3, flattenedResources);
    }

    [Fact]
    public async Task FilterResources_WhenTeacherTeachesMultipleYearLevelsAndResourcesInCache_ShouldProvidSeparateResultsForEachYearLevel()
    {
        using var db = GetTestDb();
        var resourceService = await GetTestResourceService(db);
        var file = new Mock<IBrowserFile>();
        var ct = new CancellationToken();
        var yearLevels1 = new List<YearLevel> { YearLevel.Year1 };
        var yearLevels2 = new List<YearLevel> { YearLevel.Year2 };
        var yearLevels3 = new List<YearLevel> { YearLevel.Year3 };
        var subject = _subjects.First(s => s.Name == "Mathematics");
        var resource1 = await resourceService.UploadResourceAsync(file.Object, "Resource 1", [subject], yearLevels1, ResourceType.Video, [], ct);
        var resource2 = await resourceService.UploadResourceAsync(file.Object, "Resource 2", [subject], yearLevels2, ResourceType.Video, [], ct);
        var resource3 = await resourceService.UploadResourceAsync(file.Object, "Resource 3", [subject], yearLevels3, ResourceType.Video, [], ct);

        var user = db.Users.First();
        db.SaveChanges();

        var filteredResources = resourceService.FilterResources(subject);
        Assert.True(filteredResources.ContainsKey(YearLevel.Year1));
        Assert.True(filteredResources.ContainsKey(YearLevel.Year2));
        Assert.False(filteredResources.ContainsKey(YearLevel.Year3));
    }

    private ApplicationDbContext GetTestDb()
    {
        var db = CreateContext();
        var subject = _subjects.First();

        var yearLevel = new CurriculumYearLevel(YearLevel.Year1, "Learning Standard");
        subject.AddYearLevel(yearLevel);
        db.Subjects.Add(subject);

        var accountSetupState = new AccountSetupState(_user.Id);
        accountSetupState.SetCalendarYear(TestYear);
        db.WeekPlannerTemplates.Add(accountSetupState.WeekPlannerTemplate);

        _user.AccountSetupState = accountSetupState;
        _user.SetLastSelectedYear(TestYear);
        _user.CompleteAccountSetup();

        var yearPlan = new YearPlan(_user.Id, accountSetupState, [subject]);
        yearPlan.AddYearLevelsTaught([YearLevel.Year1, YearLevel.Year2]);

        _user.AddYearPlan(yearPlan);

        db.Users.Add(_user);
        db.SaveChanges();

        return db;
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        return db;
    }

    private async Task<ResourceService> GetTestResourceService(ApplicationDbContext db)
    {
        var storageManager = new TestStorageManager(db, _user.Id);

        var dbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        dbContextFactory.Setup(f => f.CreateDbContext()).Returns(() => CreateContext());

        dbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => CreateContext());

        var ambient = new Mock<IAmbientDbContextAccessor<ApplicationDbContext>>();
        ambient.Setup(a => a.Current).Returns(() => CreateContext());
        var uowFactory = new UnitOfWorkFactory<ApplicationDbContext>(dbContextFactory.Object, ambient.Object);
        var userRepository = new UserRepository(dbContextFactory.Object, ambient.Object);

        return new ResourceService(storageManager, userRepository, uowFactory, CreateAppState());
    }

    private AppState CreateAppState()
    {
        var logger = new Mock<ILogger<AppState>>().Object;
        var authStateProvider = new Mock<AuthenticationStateProvider>().Object;
        var userRepo = new Mock<IUserRepository>().Object;
        var termDatesService = new Mock<ITermDatesService>().Object;
        var appState = new AppState(authStateProvider, userRepo, logger, termDatesService);
        appState.User = _user;

        return appState;
    }
}


