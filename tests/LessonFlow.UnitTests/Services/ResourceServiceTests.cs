using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Services;
using LessonFlow.Services.FileStorage;
using Microsoft.AspNetCore.Components.Forms;
using Moq;

namespace LessonFlow.UnitTests.Services;

public class ResourceServiceTests
{
    private readonly Guid _userId;
    private readonly string _resourceName;
    private readonly ResourceType _resourceType;
    private string _resourceUrl => Path.Combine("unsafe_uploads", _userId.ToString());
    private readonly Subject _subject;
    private readonly List<YearLevelValue> _yearLevels;

    public ResourceServiceTests()
    {
        _userId = Guid.NewGuid();
        _resourceName = "Sample Resource";
        _resourceType = ResourceType.Video;
        _subject = new Subject([], "Mathematics");
        _yearLevels = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };

    }

    [Fact]
    public async Task CreateResource_WhenCalledWithValidParameters_ShouldCreateResource()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subject, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.NotNull(resource);
        Assert.Equal(_resourceName, resource.Name);
        Assert.Equal(_resourceType, resource.Type);
        Assert.Equal(_resourceUrl, resource.Url);
        Assert.Equal(_userId, resource.UserId);
        Assert.Equal(_yearLevels, resource.YearLevels);
    }

    [Fact]
    public void ResourceCache_WhenNoResourcesCreated_ShouldBeEmpty()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        Assert.Empty(resourceService.ResourceCache);
    }

    [Fact]
    public void CreateResource_WhenCalled_CreatedResourceAddedToResourceCache()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var file = new Mock<IBrowserFile>();

        var resource = await resourceService.CreateResource(file.Object, _resourceName, _subject, _yearLevels, _resourceType, [], new CancellationToken());

        Assert.Single(resourceService.ResourceCache);
        Assert.Contains(resource, resourceService.ResourceCache);
    }

    [Fact]
    public void CreateResource_WhenCalledWithNullStrands_ShouldCreateResourceWithEmptyStrands()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var resource = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, null);
        Assert.Empty(resource.AssociatedTopics);
    }

    [Fact]
    public void CreateResource_WhenCalledWithStrands_ShouldCreateResourceWithGivenStrands()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var topics = new List<ConceptualOrganiser> { new ConceptualOrganiser() { Name = "Algebra" }, new ConceptualOrganiser() { Name = "Number" } };
        var resource = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, topics);
        Assert.Equal(topics, resource.AssociatedTopics);
    }

    [Fact]
    public void CreateResource_WhenAddingTwoResourcesWithSameName_ShouldCreateDistinctResources()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var resource1 = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, []);
        var resource2 = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, []);
        Assert.NotEqual(resource1.Id, resource2.Id);
        Assert.Equal(2, resourceService.ResourceCache.Count);
    }

    [Fact]
    public void DeleteResource_WhenResourceExists_ShouldRemoveFromCache()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var resource = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, []);

        resourceService.DeleteResource(resource);
        Assert.DoesNotContain(resource, resourceService.ResourceCache);
    }

    [Fact]
    public void DeleteResource_WhenTwoResourcesWithSameNameExist_ShouldRemoveOnlySpecifiedResource()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var resource1 = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, []);
        var resource2 = resourceService.CreateResource(_resourceName, _resourceUrl, _subject, _yearLevels, _resourceType, []);

        resourceService.DeleteResource(resource1);
        Assert.DoesNotContain(resource1, resourceService.ResourceCache);
        Assert.Contains(resource2, resourceService.ResourceCache);
    }

    [Fact]
    public void FilterResources_WhenSubjectIsSpecifiedAndResourcesExist_ShouldReturnOnlyResourcesOfThatSubject()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var subjectMath = new Subject([], "Mathematics");
        var subjectScience = new Subject([], "Science");
        var resource1 = resourceService.CreateResource("Math Resource", "url1", subjectMath, _yearLevels, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Science Resource", "url2", subjectScience, _yearLevels, ResourceType.Video, []);
        var filteredResources = resourceService.FilterResources(subjectMath);
        Assert.Single(filteredResources);
        Assert.Contains(resource1, filteredResources);
        Assert.DoesNotContain(resource2, filteredResources);
    }

    [Fact]
    public void FilterResources_WhenNoResourcesOfSubjectExist_ShouldReturnEmptyList()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var subjectMath = new Subject([], "Mathematics");
        var subjectScience = new Subject([], "Science");
        var resource1 = resourceService.CreateResource("Math Resource", "url1", subjectMath, _yearLevels, ResourceType.Video, []);
        var filteredResources = resourceService.FilterResources(subjectScience);
        Assert.Empty(filteredResources);
    }

    [Fact]
    public void FilterResources_WhenMultipleResourcesOfSubjectExist_ShouldReturnAllResourcesOfThatSubject()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var subjectMath = new Subject([], "Mathematics");
        var resource1 = resourceService.CreateResource("Math Resource 1", "url1", subjectMath, _yearLevels, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Math Resource 2", "url2", subjectMath, _yearLevels, ResourceType.Video, []);
        var subjectScience = new Subject([], "Science");
        var resource3 = resourceService.CreateResource("Science Resource", "url2", subjectScience, _yearLevels, ResourceType.Video, []);
        var filteredResources = resourceService.FilterResources(subjectMath);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
    }

    [Fact]
    public void FilterResources_WhenResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year2, YearLevelValue.Year3 };
        var subject = new Subject([], "Mathematics");
        var resource1 = resourceService.CreateResource("Resource 1", "url1", subject, yearLevels1, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Resource 2", "url2", subject, yearLevels2, ResourceType.Video, []);

        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Reception]);

        Assert.Single(filteredResources);
        Assert.Contains(resource1, filteredResources);
        Assert.DoesNotContain(resource2, filteredResources);
    }

    [Fact]
    public void FilterResources_WhenNoResourcesInCacheExistForSpecifiedYearLevel_ReturnEmptyList()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year2, YearLevelValue.Year3 };
        var subject = new Subject([], "Mathematics");
        var resource1 = resourceService.CreateResource("Resource 1", "url1", subject, yearLevels1, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Resource 2", "url2", subject, yearLevels2, ResourceType.Video, []);

        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year5]);

        Assert.Empty(filteredResources);
    }

    [Fact]
    public void FilterResources_WhenMultipleResourcesInCacheExistForSpecifiedYearLevel_ReturnThoseResources()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year1, YearLevelValue.Year2 };
        var subject = new Subject([], "Mathematics");
        var resource1 = resourceService.CreateResource("Resource 1", "url1", subject, yearLevels1, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Resource 2", "url2", subject, yearLevels2, ResourceType.Video, []);
        var resource3 = resourceService.CreateResource("Resource 3", "url3", subject, [YearLevelValue.Year3], ResourceType.Video, []);
        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year1]);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
        Assert.DoesNotContain(resource3, filteredResources);
    }

    [Fact]
    public void FilterResources_WhenResourcesExistForSpecifiedConceptualOrganisers_ReturnThoseResources()
    {
        var storageManager = new Mock<IStorageManager>();
        var resourceService = new ResourceService(storageManager.Object, _userId);
        var yearLevels1 = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };
        var yearLevels2 = new List<YearLevelValue> { YearLevelValue.Year1, YearLevelValue.Year2 };
        var subject = new Subject([], "Mathematics");
        var resource1 = resourceService.CreateResource("Resource 1", "url1", subject, yearLevels1, ResourceType.Video, []);
        var resource2 = resourceService.CreateResource("Resource 2", "url2", subject, yearLevels2, ResourceType.Video, []);
        var resource3 = resourceService.CreateResource("Resource 3", "url3", subject, [YearLevelValue.Year3], ResourceType.Video, []);
        var filteredResources = resourceService.FilterResources(subject, [YearLevelValue.Year1]);
        Assert.Equal(2, filteredResources.Count);
        Assert.Contains(resource1, filteredResources);
        Assert.Contains(resource2, filteredResources);
        Assert.DoesNotContain(resource3, filteredResources);
    }
}
