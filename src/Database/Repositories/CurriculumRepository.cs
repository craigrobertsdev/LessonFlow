using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class CurriculumRepository(ApplicationDbContext context) : ICurriculumRepository
{
    public async Task AddCurriculum(List<Subject> subjects, CancellationToken cancellationToken)
    {
        // clear existing curriculum subjects
        var curriculumSubjects = await context.CurriculumSubjects
            .ToListAsync(cancellationToken);

        context.CurriculumSubjects.RemoveRange(curriculumSubjects);
        await context.SaveChangesAsync(cancellationToken);

        // add new subjects
        foreach (var subject in subjects)
        {
            context.CurriculumSubjects.Add(subject);
        }
    }

    public async Task<List<Subject>> GetAllSubjects(CancellationToken cancellationToken)
    {
        return await context.CurriculumSubjects.ToListAsync(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsById(List<SubjectId> subjectIds,
        CancellationToken cancellationToken)
    {
        return await context.CurriculumSubjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsByName(List<string> subjectNames,
        CancellationToken cancellationToken)
    {
        return await context.CurriculumSubjects
            .Where(s => subjectNames.Contains(s.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsByYearLevels(List<YearLevelValue> yearLevels,
        CancellationToken cancellationToken)
    {
        {
            var subjects = await context.CurriculumSubjects
                .Include(s => s.YearLevels)
                .ThenInclude(yl => yl.ConceptualOrganisers)
                .ThenInclude(c => c.ContentDescriptions)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return subjects.Select(s =>
                new Subject(s.Name, s.RemoveYearLevelsNotTaught(yearLevels), s.Description)).ToList();
        }
    }
}