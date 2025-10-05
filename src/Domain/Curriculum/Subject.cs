using LessonFlow.Api.Contracts.Curriculum;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.Curriculum;

public class Subject : Entity<SubjectId>, IAggregateRoot
{
    private readonly List<YearLevel> _yearLevels = [];
    public string Name { get; } = string.Empty;
    public IReadOnlyList<YearLevel> YearLevels => _yearLevels.AsReadOnly();
    public string Description { get; private set; } = string.Empty;

    public void AddYearLevel(YearLevel yearLevel)
    {
        if (YearLevels.Any(yl => yl.YearLevelValue == yearLevel.YearLevelValue))
        {
            return;
        }

        _yearLevels.Add(yearLevel);
    }

    public List<YearLevel> RemoveYearLevelsNotTaught(List<YearLevelValue> yearLevels)
    {
        var redactedYearLevels = new List<YearLevel>();
        foreach (var yearLevel in _yearLevels)
        {
            if (yearLevels.Contains(yearLevel.YearLevelValue))
            {
                redactedYearLevels.Add(yearLevel);
                continue;
            }

            var subjectYearLevels = yearLevel.GetYearLevels();
            if (yearLevels.Contains(subjectYearLevels[0]))
            {
                redactedYearLevels.Add(yearLevel);
            }
            else if (subjectYearLevels.Length > 1 && yearLevels.Contains(subjectYearLevels[1]))
            {
                redactedYearLevels.Add(yearLevel);
            }
        }

        return redactedYearLevels;
    }

    public Subject(List<YearLevel> yearLevels, string name)
    {
        Id = new SubjectId(Guid.NewGuid());
        _yearLevels = yearLevels;
        Name = name;
    }

    public Subject(string name, List<YearLevel> yearLevels, string description)
    {
        Id = new SubjectId(Guid.NewGuid());
        Name = name;
        _yearLevels = yearLevels;
        Description = description;
    }

    private Subject() { }
}

public static class CurriculumSubjectExtensions
{
    public static Subject FilterYearLevels(this Subject subject,
        IEnumerable<YearLevelValue> yearLevelValues)
    {
        subject.FilterYearLevels(yearLevelValues);
        return subject;
    }

    public static Subject FilterYearLevels(this Subject subject, YearLevelValue yearLevelValue)
    {
        subject.FilterYearLevels(yearLevelValue);
        return subject;
    }

    public static Subject FilterContentDescriptions(this Subject subject,
        IEnumerable<Guid> contentDescriptionIds)
    {
        subject.YearLevels.FilterContentDescriptions(contentDescriptionIds);
        return subject;
    }
}

public static class CurriculumSubjectDtoExtensions
{
    public static CurriculumSubjectDto ToDto(this Subject s)
    {
        return new CurriculumSubjectDto(s.Id.Value, s.Name, s.YearLevels.Select(yl => yl.ToDto()).ToList());
    }

    public static YearLevelDto ToDto(this YearLevel yl, bool withAllInformation = true)
    {
        var capabilities = withAllInformation ? yl.Capabilities.Select(c => c.ToDto()) : new List<CapabilityDto>();
        var dispositions = withAllInformation ? yl.Dispositions.Select(d => d.ToDto()) : new List<DispositionDto>();
        var conceptualOrganisers = yl.ConceptualOrganisers.Select(co => co.ToDto(withAllInformation));

        return new YearLevelDto(yl.YearLevelValue,
            yl.LearningStandard,
            capabilities,
            dispositions,
            conceptualOrganisers);
    }

    private static CapabilityDto ToDto(this Capability c)
    {
        return new CapabilityDto(c.Name, c.Descriptors);
    }

    private static DispositionDto ToDto(this Disposition d)
    {
        return new DispositionDto(d.Title, d.DevelopedWhen);
    }

    public static ConceptualOrganiserDto ToDto(this ConceptualOrganiser co, bool withAllInformation = true)
    {
        if (withAllInformation)
        {
            return new ConceptualOrganiserDto(
                co.Name,
                co.WhatItIs,
                co.WhyItMatters,
                co.ConceptualUnderstandings,
                co.ContentDescriptions.Select(cd => cd.ToDto(false)));
        }

        return new ConceptualOrganiserDto(
            string.Empty,
            string.Empty,
            string.Empty,
            [],
            co.ContentDescriptions.Select(cd => cd.ToDto(false)));
    }

    public static ContentDescriptionDto ToDto(this ContentDescription cd, bool withAllInformation = true)
    {
        var text = withAllInformation ? cd.Text : string.Empty;
        var curriculumCodes = withAllInformation ? cd.CurriculumCodes : [];

        return new ContentDescriptionDto(cd.Id, text, cd.ConceptualOrganiser.Name, curriculumCodes);
    }
}