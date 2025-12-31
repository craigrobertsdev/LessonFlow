using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Resources;
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
        await using var context = await factory.CreateDbContextAsync(ct);
        var user = await context.Users
            .Where(u => u.Email == email)
            //.Include(u => u.AccountSetupState)
            //.Include(u => u.Resources)
            //.Include(u => u.YearPlans)
            //.AsSplitQuery()
            //.AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return null;
        }

        if (user.AccountSetupComplete)
        {
            return await context.Users
                .Where(u => u.Email == email)
                .Include(u => u.Resources)
                .Include(u => u.YearPlans)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            return await context.Users
                .Where(u => u.Email == email)
                .Include(u => u.AccountSetupState)
                .ThenInclude(a => a!.WeekPlannerTemplate)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
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

    public async Task UpdateAccountSetupState(Guid userId, AccountSetupState newState, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(UpdateAccountSetupState)} must be called with a UnitOfWork");

        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccountSetupState)
                .ThenInclude(a => a!.WeekPlannerTemplate)
            .FirstOrDefaultAsync(ct) ?? throw new UserNotFoundException();

        if (user.AccountSetupState is null)
        {
            user.AccountSetupState = newState;
            await context.SaveChangesAsync(ct);
            return;
        }

        var weekPlannerTemplate = await context.WeekPlannerTemplates
            .Where(wp => wp.Id == user.AccountSetupState.WeekPlannerTemplate.Id)
            .FirstOrDefaultAsync(ct);

        user.AccountSetupState.Update(newState);


        await context.SaveChangesAsync(ct);
    }

    public async Task<YearPlan> CompleteAccountSetup(Guid userId, AccountSetupState accountSetupState, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(CompleteAccountSetup)} must be called with a UnitOfWork");
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.AccountSetupState)
                .ThenInclude(a => a!.WeekPlannerTemplate)
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            throw new Exception("User not found");
        }

        var subjects = await context.Subjects
            .Where(s => accountSetupState.SubjectsTaught.Contains(s.Name))
            .ToListAsync(ct);

        var yearPlan = new YearPlan(user.Id, accountSetupState, subjects);

        user.CompleteAccountSetup();

        var weekPlannerTemplate = await context.WeekPlannerTemplates
            .Where(wp => wp.Id == accountSetupState.WeekPlannerTemplate.Id)
            .FirstOrDefaultAsync(ct);

        if (weekPlannerTemplate is null)
        {
            context.WeekPlannerTemplates.Add(accountSetupState.WeekPlannerTemplate);
        }
        else
        {
            weekPlannerTemplate.UpdateFrom(accountSetupState.WeekPlannerTemplate);
        }
        await context.SaveChangesAsync(ct);

        context.YearPlans.Add(yearPlan);
        await context.SaveChangesAsync(ct);

        return yearPlan;
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
            .Include(u => u.Resources)
            .ThenInclude(r => r.Subjects)
            .Include(u => u.YearPlans)
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
                .Where(r => r.Subjects.Any(s => s.Id == subjectId)))
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
        var yearPlan = await context.YearPlans
            .Where(y => y.UserId == userId && y.CalendarYear == calendarYear)
            .Include(yd => yd.WeekPlannerTemplate)
            .Include(yd => yd.SubjectsTaught)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return yearPlan;
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

    public async Task AddResourceAsync(User user, Resource resource, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(CompleteAccountSetup)} must be called with a UnitOfWork");
        var dbUser = await context.Users
            .Where(u => u.Id == user.Id)
            .Include(u => u.Resources)
            .FirstOrDefaultAsync(ct) ?? throw new UserNotFoundException();

        var subjects = context.Subjects.Where(s => resource.Subjects.Contains(s))
            .ToList();

        resource.Subjects.Clear();
        resource.Subjects.AddRange(subjects);
        dbUser.StorageUsed = user.StorageUsed;

        context.Resources.Add(resource);
    }

    public async Task SoftDeleteResourceAsync(Resource resource, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(SoftDeleteResourceAsync)} must be called with a UnitOfWork");
        var dbResource = await context.Resources
            .Where(r => r.Id == resource.Id)
            .FirstOrDefaultAsync(ct);

        if (dbResource is null)
        {
            throw new ResourceNotFoundException(resource.Id);
        }

        dbResource.MarkAsDeleted();
    }

    public async Task<List<Resource>> GetSoftDeletedResourcesAsync(Guid userId, CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Resources
            .Where(r => r.UserId == userId && r.IsSoftDeleted && r.DeletionDate >= DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task<List<Resource>> GetResourcesDueForDeletionAsync(CancellationToken ct)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Resources
            .Where(r => r.IsSoftDeleted && r.DeletionDate <= DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task HardDeleteResourcesAsync(IEnumerable<ResourceId> resourceIds, CancellationToken ct)
    {
        var context = ambient.Current ?? throw new InvalidOperationException($"{nameof(HardDeleteResourcesAsync)} must be called with a UnitOfWork");
        var resourcesToDelete = await context.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .ToListAsync(ct);

        context.Resources.RemoveRange(resourcesToDelete);
    }
}