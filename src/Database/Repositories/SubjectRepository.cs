using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class SubjectRepository(IDbContextFactory<ApplicationDbContext> factory) : ISubjectRepository
{
    public async Task AddCurriculum(List<Subject> subjects, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var curriculumSubjects = await context.Subjects
            .ToListAsync(ct);

        context.Subjects.RemoveRange(curriculumSubjects);
        await context.SaveChangesAsync(ct);

        foreach (var subject in subjects)
        {
            context.Subjects.Add(subject);
        }
    }

    public async Task<List<Subject>> GetAllSubjects(CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Subjects.ToListAsync(ct);
    }

    public async Task<List<Subject>> GetSubjectsById(List<SubjectId> subjectIds,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(ct);
    }

    public async Task<List<Subject>> GetSubjectsByName(List<string> subjectNames,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Subjects
            .Where(s => subjectNames.Contains(s.Name))
            .ToListAsync(ct);
    }

    public async Task<Subject?> GetSubjectById(SubjectId subjectId, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var subject = await context.Subjects
            .Where(s => s.Id == subjectId)
            .FirstOrDefaultAsync(ct);

        return subject;
    }

    public async Task<List<Subject>> GetSubjectsByYearLevels(List<YearLevel> yearLevels,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var subjects = await context.Subjects
            .Include(s => s.YearLevels)
            .ThenInclude(yl => yl.ConceptualOrganisers)
            .ThenInclude(c => c.ContentDescriptions)
            .AsNoTracking()
            .ToListAsync(ct);

        return subjects.Select(s =>
            new Subject(s.Name, s.RemoveYearLevelsNotTaught(yearLevels), s.Description)).ToList();
    }
}