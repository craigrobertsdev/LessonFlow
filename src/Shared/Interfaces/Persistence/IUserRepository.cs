using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Resources;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearPlans;

namespace LessonFlow.Shared.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email, CancellationToken ct);
    Task<AccountSetupState?> GetAccountSetupState(Guid userId, CancellationToken ct);
    Task UpdateAccountSetupState(Guid userId, AccountSetupState accountSetupState, CancellationToken ct);
    Task<YearPlan> CompleteAccountSetup(Guid userId, AccountSetupState accountSetupState, CancellationToken ct);
    Task<User?> GetById(Guid userId, CancellationToken ct);
    Task<User?> GetWithResources(Guid userId, CancellationToken ct);
    Task<User?> GetByIdWithResources(Guid userId, IEnumerable<ResourceId> resources, CancellationToken ct);
    Task<List<Resource>> GetResourcesBySubject(Guid userId, SubjectId subjectId, CancellationToken ct);
    Task<List<Resource>> GetResourcesById(IEnumerable<ResourceId> resourceIds, CancellationToken ct);
    Task<List<Subject>> GetSubjectsTaughtByUserWithoutElaborations(Guid userId, CancellationToken ct);
    Task<List<Subject>> GetSubjectsTaughtByUserWithElaborations(Guid userId, CancellationToken ct);
    Task<YearPlan?> GetYearPlanByYear(Guid userId, int calendarYear, CancellationToken ct);
    void Delete(User user);
    Task AddResourceAsync(User user, Resource resource, CancellationToken ct);
    Task SoftDeleteResourceAsync(Resource resource, CancellationToken ct);
    Task<List<Resource>> GetSoftDeletedResourcesAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Gets the resources pending deletion across all users.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<Resource>> GetResourcesDueForDeletionAsync(CancellationToken ct);
    Task HardDeleteResourcesAsync(IEnumerable<ResourceId> resourceIds, CancellationToken ct);
}