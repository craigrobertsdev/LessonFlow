using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class SubjectRepository(ApplicationDbContext context) : ISubjectRepository
{
    public async Task AddCurriculum(List<Subject> subjects, CancellationToken cancellationToken)
    {
        var curriculumSubjects = await context.Subjects
            .ToListAsync(cancellationToken);

        context.Subjects.RemoveRange(curriculumSubjects);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var subject in subjects)
        {
            context.Subjects.Add(subject);
        }
    }

    public async Task<List<Subject>> GetAllSubjects(CancellationToken cancellationToken)
    {
        return await context.Subjects.ToListAsync(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsById(List<SubjectId> subjectIds,
        CancellationToken cancellationToken)
    {
        return await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsByName(List<string> subjectNames,
        CancellationToken cancellationToken)
    {
        return await context.Subjects
            .Where(s => subjectNames.Contains(s.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<Subject?> GetSubjectById(SubjectId subjectId, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects
            .Where(s => s.Id == subjectId)
            .FirstOrDefaultAsync(cancellationToken);

        return subject;
    }

    public async Task<List<Subject>> GetSubjectsByYearLevels(List<YearLevelValue> yearLevels,
        CancellationToken cancellationToken)
    {
        {
            var subjects = await context.Subjects
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