using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Interfaces.Persistence;

public interface IPlannerTemplateRepository
{
    Task<WeekPlannerTemplate?> GetById(WeekPlannerTemplateId id, CancellationToken cancellationToken);
    Task<WeekPlannerTemplate?> GetByTeacherId(Guid UserId, CancellationToken cancellationToken);
    void Add(WeekPlannerTemplate weekPlannerTemplate);
}