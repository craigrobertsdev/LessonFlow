using LessonFlow.Api.Contracts.Curriculum;
using LessonFlow.Api.Contracts.LessonPlans;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.Domain.LessonPlans;

public sealed class LessonPlan : Entity<LessonPlanId>, IAggregateRoot
{
    private readonly List<LessonComment> _comments = [];
    private readonly List<Resource> _resources = [];

    public YearData YearData { get; private set; }
    public Subject Subject { get; private set; }
    public string PlanningNotes { get; private set; }
    public string PlanningNotesHtml { get; private set; }
    public DateOnly LessonDate { get; private set; }
    public int NumberOfLessons { get; private set; }
    public int StartPeriod { get; private set; }
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }
    public IReadOnlyList<Resource> Resources => _resources.AsReadOnly();
    public IReadOnlyList<LessonComment> Comments => _comments.AsReadOnly();

    public void AddResource(Resource resource)
    {
        if (!_resources.Contains(resource))
        {
            _resources.Add(resource);
            UpdatedDateTime = DateTime.UtcNow;
        }
    }

    public void SetNumberOfLessons(int newNumberOfLessons)
    {
        if (newNumberOfLessons == NumberOfLessons)
        {
            return;
        }

        NumberOfLessons = newNumberOfLessons;
        UpdatedDateTime = DateTime.UtcNow;
    }

    public void SetStartPeriod(int startPeriod)
    {
        if (startPeriod == StartPeriod)
        {
            return;
        }

        StartPeriod = startPeriod;
        UpdatedDateTime = DateTime.UtcNow;
    }

    public void UpdateSubject(Subject subject)
    {
        if (Subject.Id == subject.Id)
        {
            return;
        }

        Subject = subject;
        UpdatedDateTime = DateTime.UtcNow;
    }

    public void SetPlanningNotes(string newPlanningNotes, string newPlanningNotesHtml)
    {
        (PlanningNotes, PlanningNotesHtml) = (newPlanningNotes, newPlanningNotesHtml);
    }

    public void UpdateResources(IEnumerable<Resource> resources)
    {
        if (!resources.Any())
        {
            _resources.Clear();
            return;
        }

        var resourcesToRemove = _resources.Where(r => !resources.Contains(r)).ToList();
        var resourcesToAdd = resources.Where(r => !_resources.Contains(r)).ToList();
        _resources.RemoveAll(resourcesToRemove.Contains);
        _resources.AddRange(resourcesToAdd);
    }

    public void ClearResources()
    {
        _resources.Clear();
    }

    public IEnumerable<Resource> MatchResources(IEnumerable<Resource> resources)
    {
        return resources.Where(_resources.Contains);
    }

    public LessonPlan(
        YearData yearData,
        Subject subject,
        List<Guid> contentDescriptionIds,
        string planningNotes,
        string planningNotesHtml,
        int numberOfPeriods,
        int startPeriod,
        DateOnly lessonDate,
        DateTime createdDateTime,
        DateTime updatedDateTime,
        List<Resource>? resources)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfPeriods);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startPeriod);

        Id = new LessonPlanId(Guid.NewGuid());
        YearData = yearData;
        Subject = subject;
        PlanningNotes = planningNotes;
        PlanningNotesHtml = planningNotesHtml;
        NumberOfLessons = numberOfPeriods;
        StartPeriod = startPeriod;
        LessonDate = lessonDate;
        CreatedDateTime = createdDateTime;
        UpdatedDateTime = updatedDateTime;
        _resources = resources ?? [];
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private LessonPlan()
    {
    }
}

public static class LessonPlanDtoExtensions
{
    public static List<LessonCommentDto> ToDtos(this IEnumerable<LessonComment> comments)
    {
        return comments.Select(c => new LessonCommentDto(c.Content, c.Completed, c.StruckOut, c.CompletedDateTime))
            .ToList();
    }

    public static List<LessonPlanDto> ToDtos(this IEnumerable<LessonPlan> lessonPlans, IEnumerable<Resource> resources,
        IEnumerable<Subject> subjects)
    {
        return lessonPlans.Select(lp => new LessonPlanDto(
                lp.Id.Value,
                lp.MatchSubject(subjects),
                lp.PlanningNotes,
                lp.PlanningNotesHtml,
                lp.MatchResources(resources).ConvertToDtos(),
                lp.Comments.ToDtos(),
                lp.StartPeriod,
                lp.NumberOfLessons))
            .ToList();
    }

    public static LessonPlanDto ToDto(this LessonPlan lessonPlan, IEnumerable<Resource> resources,
        Subject subject)
    {
        var dto = new LessonPlanDto(
            lessonPlan.Id.Value,
            subject.ToDto(),
            lessonPlan.PlanningNotes,
            lessonPlan.PlanningNotesHtml,
            lessonPlan.MatchResources(resources).ConvertToDtos(),
            lessonPlan.Comments.ToDtos(),
            lessonPlan.StartPeriod,
            lessonPlan.NumberOfLessons);

        return dto;
    }

    public static CurriculumSubjectDto MatchSubject(this LessonPlan lessonPlan, IEnumerable<Subject> subjects)
    {
        return subjects.First(s => s.Id == lessonPlan.Subject.Id).ToDto();
    }
}