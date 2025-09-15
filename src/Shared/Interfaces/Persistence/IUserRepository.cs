using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);
    Task<AccountSetupState?> GetAccountSetupState(Guid userId, CancellationToken cancellationToken);
    Task UpdateAccountSetupState(Guid userId, AccountSetupState accountSetupState);
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetWithResources(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetByIdWithResources(Guid userId, IEnumerable<ResourceId> resources, CancellationToken cancellationToken);
    Task<List<Resource>> GetResourcesBySubject(Guid userId, SubjectId subjectId, CancellationToken cancellationToken);
    Task<List<Resource>> GetResourcesById(IEnumerable<ResourceId> resourceIds, CancellationToken cancellationToken);
    Task<List<Subject>> GetSubjectsTaughtByUserWithoutElaborations(Guid userId, CancellationToken cancellationToken);
    Task<List<Subject>> GetSubjectsTaughtByUserWithElaborations(Guid userId, CancellationToken cancellationToken);
    Task<YearData?> GetYearDataByYear(Guid userId, int calendarYear, CancellationToken cancellationToken);
    void Delete(User user);
}