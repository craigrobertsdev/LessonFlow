using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LessonFlow.Database.Repositories;

public class UserRepository(IDbContextFactory<ApplicationDbContext> factory, IAmbientDbContextAccessor<ApplicationDbContext> ambient) : IUserRepository
{
    public async Task<User?> GetByEmail(string email, CancellationToken ct)
    {
        try
        {
            await using var context = await factory.CreateDbContextAsync(ct);
            var user = await context.Users
                .Where(u => u.Email == email)
                .Include(u => u.AccountSetupState)
                .Include(u => u.Resources)
                .Include(u => u.YearPlans)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            //if (user is null) return null;

            //user.YearPlans.ForEach(yd =>
            //{
            //    foreach (var subject in yd.SubjectsTaught)
            //    {
            //        context.Entry(subject).State = EntityState.Detached;
            //    }
            //});

            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<AccountSetupState?> GetAccountSetupState(Guid userId,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.AccountSetupState
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(ct) ?? throw new UserNotFoundException();

        return user.AccountSetupState;
    }

    public async Task UpdateAccountSetupState(Guid userId, AccountSetupState accountSetupState, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(UpdateAccountSetupState)} must be called with a UnitOfWork");
        var user = await context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(ct) ?? throw new UserNotFoundException();

        user.AccountSetupState = accountSetupState;
        await context.SaveChangesAsync(ct);
    }

    public async Task CompleteAccountSetup(Guid userId, YearPlan yearPlan, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.YearPlans)
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

    public async Task<User?> GetById(Guid userId, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccountSetupState)
            .Include(u => u.Resources)
            .Include(u => u.YearPlans)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByIdWithResources(Guid userId, IEnumerable<ResourceId> resources,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources.Where(r => resources.Contains(r.Id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetWithResources(Guid userId, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Resource>> GetResourcesBySubject(Guid userId, SubjectId subjectId,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(t => t.Resources
                .Where(r => r.Subject.Id == subjectId))
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return user != null ? [.. user.Resources] : [];
    }

    public Task<List<Subject>> GetSubjectsTaughtByUserWithElaborations(Guid userId,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<List<Subject>> GetSubjectsTaughtByUserWithoutElaborations(Guid userId,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<YearPlan?> GetYearPlanByYear(Guid userId, int calendarYear,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.YearPlans
            .Where(y => y.UserId == userId && y.CalendarYear == calendarYear)
            .Include(yd => yd.WeekPlannerTemplate)
            .Include(yd => yd.SubjectsTaught)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public void Delete(User user)
    {
        using var context = factory.CreateDbContext();
        context.Users.Remove(user);
    }

    public async Task<List<Resource>> GetResourcesById(IEnumerable<ResourceId> resourceIds,
        CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .AsNoTracking()
            .ToListAsync(ct);
    }
}