using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public void Update(User user)
    {
        context.Users.Update(user);
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .Where(u => u.Email == email)
                .Include(u => u.AccountSetupState)
                .Include(u => u.Resources)
                .Include(u => u.YearPlanHistory)
                .AsSplitQuery()
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null) return null;

            user.YearPlanHistory.ForEach(yd =>
            {
                foreach (var subject in yd.SubjectsTaught)
                {
                    context.Entry(subject).State = EntityState.Detached;
                }
            });

            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<AccountSetupState?> GetAccountSetupState(Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.AccountSetupState
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        return user.AccountSetupState ?? null;
    }

    public async Task UpdateAccountSetupState(Guid userId, AccountSetupState accountSetupState)
    {
        var user = await context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new Exception("User not found");
        }

        user.AccountSetupState = accountSetupState;
        await context.SaveChangesAsync();
    }

    public async Task CompleteAccountSetup(Guid userId, YearPlan yearPlan)
    {
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.YearPlanHistory)
            .Include(u => u.AccountSetupState)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new Exception("User not found");
        }

        user.CompleteAccountSetup();
        user.AddYearPlan(yearPlan);

        await context.SaveChangesAsync();
    }

    public async Task<User?> GetById(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccountSetupState)
            .Include(u => u.Resources)
            .Include(u => u.YearPlanHistory)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByIdWithResources(Guid userId, IEnumerable<ResourceId> resources,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources.Where(r => resources.Contains(r.Id)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetWithResources(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Resource>> GetResourcesBySubject(Guid userId, SubjectId subjectId,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources
                .Where(r => r.Subject.Id == subjectId))
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        return user != null ? [.. user.Resources] : [];
    }

    public Task<List<Subject>> GetSubjectsTaughtByUserWithElaborations(Guid userId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<Subject>> GetSubjectsTaughtByUserWithoutElaborations(Guid userId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<YearPlan?> GetYearPlanByYear(Guid userId, int calendarYear,
        CancellationToken cancellationToken)
    {
        return await context.YearPlans
            .Where(y => y.UserId == userId && y.CalendarYear == calendarYear)
            .Include(yd => yd.WeekPlannerTemplate)
            .Include(yd => yd.SubjectsTaught)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Delete(User user)
    {
        context.Users.Remove(user);
    }

    public async Task<List<Resource>> GetResourcesById(IEnumerable<ResourceId> resourceIds,
        CancellationToken cancellationToken)
    {
        return await context.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }
}