using LessonFlow.Api.Contracts.Curriculum;
using LessonFlow.Api.Contracts.LessonPlans;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared.Interfaces;

namespace LessonFlow.Domain.LessonPlans;

public sealed class LessonPlan : Entity<LessonPlanId>, IAggregateRoot, ILessonPeriod, IPlannerPeriod
{
    public DayPlanId DayPlanId { get; private set; }
    public Subject Subject { get; private set; }
    public PeriodType PeriodType { get; private set; }
    public string PlanningNotesHtml { get; internal set; }
    public DateOnly LessonDate { get; private set; }
    public int NumberOfPeriods { get; private set; }
    public int StartPeriod { get; private set; }
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }
    public List<Resource> Resources { get; private set; } = [];
    public List<LessonComment> Comments { get; private set; } = [];
    public string SubjectName => Subject.Name;
    public List<TodoItem> ToDos { get; private set; } = [];
    public bool LessonIsPlanned => true;

    public void AddResource(Resource resource)
    {
        if (!Resources.Contains(resource))
        {
            Resources.Add(resource);
            UpdatedDateTime = DateTime.UtcNow;
        }
    }

    public void SetNumberOfPeriods(int newNumberOfPeriods)
    {
        if (newNumberOfPeriods == NumberOfPeriods)
        {
            return;
        }

        NumberOfPeriods = newNumberOfPeriods;
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
        Subject = subject;
        UpdatedDateTime = DateTime.UtcNow;
    }

    public void SetPlanningNotes(string newPlanningNotesHtml)
    {
        PlanningNotesHtml = newPlanningNotesHtml;
    }

    public void UpdateResources(IEnumerable<Resource> resources)
    {
        if (!resources.Any())
        {
            Resources.Clear();
            return;
        }

        var resourcesToRemove = Resources.Where(r => !resources.Contains(r)).ToList();
        var resourcesToAdd = resources.Where(r => !Resources.Contains(r)).ToList();
        Resources.RemoveAll(resourcesToRemove.Contains);
        Resources.AddRange(resourcesToAdd);
    }

    public void ClearResources()
    {
        Resources.Clear();
    }

    public IEnumerable<Resource> MatchResources(IEnumerable<Resource> resources)
    {
        return resources.Where(Resources.Contains);
    }

    public LessonPlan Clone()
    {
        return new LessonPlan(
            Id,
            DayPlanId,
            Subject,
            PeriodType,
            PlanningNotesHtml,
            NumberOfPeriods,
            StartPeriod,
            LessonDate,
            [.. Resources]);
    }

    public void UpdateValuesFrom(LessonPlan other)
    {
        UpdateSubject(other.Subject);
        SetNumberOfPeriods(other.NumberOfPeriods);
        UpdateResources(other.Resources);
    }

    public LessonPlan(
        DayPlanId dayPlanId,
        Subject subject,
        PeriodType periodType,
        string planningNotesHtml,
        int numberOfPeriods,
        int startPeriod,
        DateOnly lessonDate,
        List<Resource>? resources)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfPeriods);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startPeriod);

        Id = new LessonPlanId(Guid.NewGuid());
        DayPlanId = dayPlanId;
        Subject = subject;
        PeriodType = periodType;
        PlanningNotesHtml = planningNotesHtml;
        NumberOfPeriods = numberOfPeriods;
        StartPeriod = startPeriod;
        LessonDate = lessonDate;
        CreatedDateTime = DateTime.UtcNow;
        UpdatedDateTime = DateTime.UtcNow;
        Resources = resources ?? [];
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private LessonPlan()
    {
    }

    private LessonPlan(LessonPlanId id, DayPlanId dayPlanId, Subject subject, PeriodType periodType, string planningNotesHtml, int numberOfPeriods, int startPeriod, DateOnly lessonDate, List<Resource> resources) : base(id)
    {
        DayPlanId = dayPlanId;
        Subject = subject;
        PeriodType = periodType;
        PlanningNotesHtml = planningNotesHtml;
        NumberOfPeriods = numberOfPeriods;
        StartPeriod = startPeriod;
        LessonDate = lessonDate;
        Resources = resources;
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
                lp.PlanningNotesHtml,
                lp.MatchResources(resources).ConvertToDtos(),
                lp.Comments.ToDtos(),
                lp.StartPeriod,
                lp.NumberOfPeriods))
            .ToList();
    }

    public static LessonPlanDto ToDto(this LessonPlan lessonPlan, IEnumerable<Resource> resources,
        Subject subject)
    {
        var dto = new LessonPlanDto(
            lessonPlan.Id.Value,
            subject.ToDto(),
            lessonPlan.PlanningNotesHtml,
            lessonPlan.MatchResources(resources).ConvertToDtos(),
            lessonPlan.Comments.ToDtos(),
            lessonPlan.StartPeriod,
            lessonPlan.NumberOfPeriods);

        return dto;
    }

    public static CurriculumSubjectDto MatchSubject(this LessonPlan lessonPlan, IEnumerable<Subject> subjects)
    {
        return subjects.First(s => s.Id == lessonPlan.Subject.Id).ToDto();
    }
}