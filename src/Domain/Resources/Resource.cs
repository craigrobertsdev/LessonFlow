using LessonFlow.Api.Contracts.Resources;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Services.FileStorage;
using LessonFlow.Shared;

namespace LessonFlow.Domain.Resources;

public sealed class Resource : Entity<ResourceId>
{
    public Resource(
        Guid userId,
        string fileName,
        string displayName,
        long fileSize,
        string link,
        ResourceType type,
        List<Subject>? associatedSubjects = null,
        List<YearLevel>? yearLevels = null,
        List<ConceptualOrganiser>? associatedConceptualOrganisers = null)
    {
        Id = new ResourceId(Guid.NewGuid());
        UserId = userId;
        FileName = fileName;
        DisplayName = displayName;
        FileSize = fileSize;
        Link = link;
        YearLevels = yearLevels ?? [];
        Subjects = associatedSubjects ?? [];
        ConceptualOrganisers = associatedConceptualOrganisers ?? [];
        Type = type;
        CreatedDateTime = DateTime.UtcNow;
        UpdatedDateTime = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string FileName { get; private set; }
    public string DisplayName { get; private set; }
    public FileSystemDirectory Directory { get; set; }
    public long FileSize { get; init; }
    public string Link { get; private set; }
    public ResourceType Type { get; set; }
    public List<LessonPlan> LessonPlans { get; private set; } = [];
    public List<Subject> Subjects { get; private set; } = [];
    public List<YearLevel> YearLevels { get; private set; } = [];
    public List<ConceptualOrganiser> ConceptualOrganisers { get; private set; } = [];
    public bool IsSoftDeleted { get; private set; } = false;
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }
    public DateTime DeletionDate { get; private set; }

    public void MarkAsDeleted()
    {
        IsSoftDeleted = true;
        UpdatedDateTime = DateTime.UtcNow;
        DeletionDate = DateTime.UtcNow.AddDays(AppConstants.SOFT_DELETION_PERIOD_DAYS);
    }

    public void UnmarkAsDeleted()
    {
        IsSoftDeleted = false;
        UpdatedDateTime = DateTime.UtcNow;
        DeletionDate = DateTime.MinValue;
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
        return new ResourceDto(resource.Id, resource.FileName, resource.Link, resource.Type, resource.YearLevels);
    }

    public static List<ResourceDto> ConvertToDtos(this IEnumerable<Resource> resources)
    {
        return resources.Select(r => new ResourceDto(r.Id, r.FileName, r.Link, r.Type, r.YearLevels)).ToList();
    }
}