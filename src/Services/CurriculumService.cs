using LessonFlow.Database;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Services;

/// <summary>
///     This class is responsible for loading the curriculum subjects from the database and storing them in memory.
///     Will be created as a singleton service in the DI container and be the source of truth for all curriculum subjects.
/// </summary>
public sealed class CurriculumService : ICurriculumService
{
    private readonly IServiceProvider _serviceProvider;

    public CurriculumService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        CurriculumSubjects = LoadCurriculumSubjects();
    }

    public List<Subject> CurriculumSubjects { get; }

    public List<string> GetSubjectNames()
    {
        return CurriculumSubjects.Select(x => x.Name).ToList();
    }

    public List<Subject> GetSubjectsByName(IEnumerable<string> names)
    {
        return CurriculumSubjects.Where(x => names.Contains(x.Name)).ToList();
    }

    public List<Subject> GetSubjectsByYearLevel(YearLevelValue yearLevel)
    {
        return CurriculumSubjects.Select(s => s.FilterYearLevels(yearLevel)).ToList();
    }

    public List<Subject> GetSubjectsByYearLevels(IEnumerable<SubjectId> subjectIds,
        IEnumerable<YearLevelValue> yearLevelValues)
    {
        var filteredSubjects = new List<Subject>();
        foreach (var subjectId in subjectIds)
        {
            var yearLevels = CurriculumSubjects.First(s => s.Id == subjectId).YearLevels
                .Where(yl => yearLevelValues.Contains(yl.YearLevelValue));
            
            var subject = CurriculumSubjects.First(s => s.Id == subjectId);
            
            filteredSubjects.Add(new Subject(subject.Name, yearLevels.ToList(), subject.Description));
        }

        return filteredSubjects;
    }

    public string GetSubjectName(SubjectId subjectId)
    {
        return CurriculumSubjects.FirstOrDefault(s => s.Id == subjectId)?.Name ?? string.Empty;
    }

    /// <summary>
    ///     Get the content descriptions for a given query and year levels.
    /// </summary>
    /// <param name="subjectId"></param>
    /// <param name="yearLevels"></param>
    /// <returns>A collection of lists of content descriptions, one for each YearLevelValue passed</returns>
    public Dictionary<YearLevelValue, List<ContentDescription>> GetContentDescriptions(SubjectId subjectId,
        List<YearLevelValue> yearLevels)
    {
        var filteredYearLevels = CurriculumSubjects
            .Where(s => s.Id == subjectId)
            .SelectMany(s => s.YearLevels)
            .Where(yl => yearLevels.Contains(yl.YearLevelValue) || yl.GetYearLevels().Intersect(yearLevels).Any())
            .ToList();

        var yearLevelContentDescriptions = new Dictionary<YearLevelValue, List<ContentDescription>>();
        foreach (var yl in yearLevels)
        {
            var yearLevel = filteredYearLevels.GetFromYearLevelValue(yl);
            var contentDescriptions = yearLevel!.GetContentDescriptions();
            yearLevelContentDescriptions.Add(yl, contentDescriptions);
        }

        return yearLevelContentDescriptions;
    }

    private List<Subject> LoadCurriculumSubjects()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subjects = context.CurriculumSubjects
            .Include(c => c.YearLevels)
            .ThenInclude(yl => yl.ConceptualOrganisers)
            .ThenInclude(s => s.ContentDescriptions)
            .AsNoTracking()
            .ToList();
        return subjects;
    }
}