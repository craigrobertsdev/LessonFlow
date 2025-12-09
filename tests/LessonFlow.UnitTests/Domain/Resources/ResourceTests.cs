using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.Resources;

namespace LessonFlow.UnitTests.Domain.Resources;
public class ResourceTests
{
    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldCreateResource()
    {
        var userId = Guid.NewGuid();
        var resourceName = "Sample Resource";
        var resourceType = ResourceType.Video;
        var resourceUrl = "https://example.com/resource";
        var subject = new Subject([], "Mathematics");
        var yearLevels = new List<YearLevelValue> { YearLevelValue.Reception, YearLevelValue.Year1 };

        var resource = new Resource(userId, resourceName, resourceUrl, subject, yearLevels, resourceType);

        Assert.Equal(resourceName, resource.Name);
        Assert.Equal(resourceType, resource.Type);
        Assert.Equal(resourceUrl, resource.Url);
        Assert.Equal(userId, resource.UserId);
        Assert.Equal(yearLevels, resource.YearLevels);
    }
}
