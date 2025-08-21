namespace LessonFlow.Domain.Curriculum;

public record ContentDescription
{
    public Guid Id { get; set; }
    public ConceptualOrganiser ConceptualOrganiser { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public string[] CurriculumCodes { get; set; } = [];
}

public static class ContentDescriptionExtensions
{
    public static List<ContentDescription> Filter(this IEnumerable<ContentDescription> cds, IEnumerable<Guid> ids)
    {
        var filteredCds = new List<ContentDescription>();
        foreach (var cd in cds)
        {
            if (ids.Contains(cd.Id))
            {
                filteredCds.Add(cd);
            }
        }

        return filteredCds;
    }
}