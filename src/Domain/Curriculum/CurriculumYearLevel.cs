using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.Curriculum;

public sealed class CurriculumYearLevel
{
    public List<Capability> Capabilities { get; private set; } = [];
    public List<Disposition> Dispositions { get; private set; } = [];
    public List<ConceptualOrganiser> ConceptualOrganisers { get; private set; } = [];
    public string LearningStandard { get; } = string.Empty;
    public string Name => YearLevelValue.ToDisplayString();
    public YearLevel YearLevelValue { get; }

    public YearLevel[] GetYearLevels()
    {
        if ((int)YearLevelValue < 15)
        {
            return [YearLevelValue];
        }

        return YearLevelValue switch
        {
            YearLevel.Years1To2 => [YearLevel.Year1, YearLevel.Year2],
            YearLevel.Years3To4 => [YearLevel.Year3, YearLevel.Year4],
            YearLevel.Years5To6 => [YearLevel.Year5, YearLevel.Year6],
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public List<ContentDescription> GetContentDescriptions()
    {
        var cds = new List<ContentDescription>();
        foreach (var co in ConceptualOrganisers)
        {
            cds.AddRange(co.ContentDescriptions);
        }

        return cds;
    }

    public void SetCapabilities(List<Capability> capabilities)
    {
        Capabilities = capabilities;
    }

    public void SetDispositions(List<Disposition> dispositions)
    {
        Dispositions = dispositions;
    }

    public void SetConceptualOrganisers(List<ConceptualOrganiser> conceptualOrganisers)
    {
        ConceptualOrganisers = conceptualOrganisers;
    }

    public CurriculumYearLevel(YearLevel yearLevelValue, string learningStandard)
    {
        YearLevelValue = yearLevelValue;
        LearningStandard = learningStandard;
    }

    public CurriculumYearLevel(
        YearLevel yearLevelValue,
        string learningStandard,
        List<Capability> capabilities,
        List<Disposition> dispositions,
        List<ConceptualOrganiser> conceptualOrganisers)
    {
        YearLevelValue = yearLevelValue;
        LearningStandard = learningStandard;
        Capabilities = capabilities;
        Dispositions = dispositions;
        ConceptualOrganisers = conceptualOrganisers;
    }

    private CurriculumYearLevel()
    {
    }
}

public static class YearLevelExtensions
{
    public static CurriculumYearLevel? GetFromYearLevelValue(this IEnumerable<CurriculumYearLevel> yearLevels,
        YearLevel yearLevelValue)
    {
        return yearLevels.FirstOrDefault(yl => yl.YearLevelValue == yearLevelValue
                                               || yl.GetYearLevels().Contains(yearLevelValue));
    }

    public static List<CurriculumYearLevel> FilterYearLevels(this IEnumerable<CurriculumYearLevel> yearLevels,
        IEnumerable<YearLevel> yearLevelValues)
    {
        return yearLevels.Where(s =>
                yearLevelValues.Contains(s.YearLevelValue))
            .ToList();
    }

    public static List<CurriculumYearLevel> FilterYearLevels(this IEnumerable<CurriculumYearLevel> yearLevels,
        YearLevel yearLevelValue)
    {
        return yearLevels.Where(yl =>
                yearLevelValue == yl.YearLevelValue
                || yearLevelValue == yl.GetYearLevels()[0]
                || yearLevelValue == yl.GetYearLevels()[1])
            .ToList();
    }

    public static void FilterContentDescriptions(this IEnumerable<CurriculumYearLevel> yearLevels,
        IEnumerable<Guid> contentDescriptionIds)
    {
        foreach (var yl in yearLevels)
        {
            yl.FilterContentDescriptions(contentDescriptionIds);
        }
    }

    public static void FilterContentDescriptions(this CurriculumYearLevel yl, IEnumerable<Guid> contentDescriptionIds)
    {
        yl.SetConceptualOrganisers(yl.ConceptualOrganisers.FilterContentDescriptions(contentDescriptionIds));
    }
}