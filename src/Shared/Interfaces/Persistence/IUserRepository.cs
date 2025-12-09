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
    Task AddResource(Guid userId, Resource resource, CancellationToken ct);
}