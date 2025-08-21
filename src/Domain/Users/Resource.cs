using LessonFlow.Api.Contracts.Resources;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.Users;

public sealed class Resource : Entity<ResourceId>
{
    private readonly List<LessonPlan> _lessonPlans = [];
    private readonly List<YearLevelValue> _yearLevels = [];

    public Guid UserId { get; private set; }
    public Subject Subject { get; private set; }
    public string Name { get; }
    public string Url { get; }
    public bool IsAssessment { get; }
    public IReadOnlyList<LessonPlan> LessonPlans => _lessonPlans.AsReadOnly();
    public IReadOnlyList<YearLevelValue> YearLevels => _yearLevels.AsReadOnly();
    public List<string> AssociatedStrands { get; private set; } = [];
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public Resource(
        Guid userId,
        string name,
        string url,
        bool isAssessment,
        Subject subject,
        List<YearLevelValue> yearLevels,
        List<string>? associatedStrands = null) 
    {
        Id = new ResourceId(Guid.NewGuid());
        UserId = userId;
        Name = name;
        Url = url;
        IsAssessment = isAssessment;
        Subject = subject;
        _yearLevels = yearLevels;

        if (associatedStrands is not null)
        {
            AssociatedStrands = associatedStrands;
        }

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
        return new ResourceDto(resource.Id, resource.Name, resource.Url, resource.IsAssessment, resource.YearLevels);
    }

    public static List<ResourceDto> ConvertToDtos(this IEnumerable<Resource> resources)
    {
        return resources.Select(r => new ResourceDto(r.Id, r.Name, r.Url, r.IsAssessment, r.YearLevels)).ToList();
    }
}