using System.Linq.Expressions;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Exceptions;
using LessonFlow.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Api.Database.Repositories;

public class SubjectRepository(ApplicationDbContext context) : ISubjectRepository
{
    public async Task<List<Subject>> GetCurriculumSubjects(
        CancellationToken cancellationToken)
    {
        return await GetSubjects(cancellationToken);
    }

    public async Task<List<Subject>> GetSubjectsById(
        List<SubjectId> subjects,
        CancellationToken cancellationToken)
    {
        Expression<Func<Subject, bool>> filter = s => subjects.Contains(s.Id);

        return await GetSubjects(cancellationToken, filter);
    }

    private async Task<List<Subject>> GetSubjects(
        CancellationToken cancellationToken,
        Expression<Func<Subject, bool>>? filter = null)
    {
        var subjectsQuery = context.CurriculumSubjects
            .AsNoTracking();

        if (filter != null)
        {
            subjectsQuery = subjectsQuery.Where(filter);
        }

        subjectsQuery = subjectsQuery
            .Include(s => s.YearLevels)
            .ThenInclude(yl => yl.ConceptualOrganisers)
            .ThenInclude(s => s.ContentDescriptions)
            .Include(s => s.YearLevels)
            .ThenInclude(yl => yl.Dispositions)
            .Include(s => s.YearLevels)
            .ThenInclude(yl => yl.Capabilities);

        var subjects = await subjectsQuery.ToListAsync(cancellationToken);

        if (subjects.Count == 0)
        {
            throw new NoSubjectsFoundException();
        }

        return subjects;
    }
}