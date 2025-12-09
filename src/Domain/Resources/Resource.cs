using LessonFlow.Api.Contracts.Resources;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.Resources;

public sealed class Resource : Entity<ResourceId>
{
    public Guid UserId { get; private set; }
    public Subject Subject { get; private set; }
    public string Name { get; }
    public string Url { get; }
    public ResourceType Type { get; set; }
    public List<LessonPlan> LessonPlans { get; private set; } = [];
    public List<YearLevelValue> YearLevels { get; private set; } = [];
    public List<ConceptualOrganiser> AssociatedTopics { get; private set; } = [];
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public Resource(
        Guid userId,
        string name,
        string url,
        Subject subject,
        List<YearLevelValue> yearLevels,
        ResourceType type,
        List<ConceptualOrganiser>? associatedTopics = null)
    {
        Id = new ResourceId(Guid.NewGuid());
        UserId = userId;
        Name = name;
        Url = url;
        Subject = subject;
        YearLevels = yearLevels;
        Type = type;
        AssociatedTopics = associatedTopics ?? [];

        CreatedDateTime = DateTime.UtcNow;
        UpdatedDateTime = DateTime.UtcNow;
    }

#pragma warning disable CS8618 // non-nullable field must contain a non-null value when exiting constructor. consider declaring as nullable.
    private Resource()
    {
    }
}

public static class ResourceDtoExtensions
{
    public static ResourceDto ConvertToDto(this Resource resource)
    {
        return new ResourceDto(resource.Id, resource.Name, resource.Url, resource.Type, resource.YearLevels);
    }

    public static List<ResourceDto> ConvertToDtos(this IEnumerable<Resource> resources)
    {
        return resources.Select(r => new ResourceDto(r.Id, r.Name, r.Url, r.Type, r.YearLevels)).ToList();
    }
}