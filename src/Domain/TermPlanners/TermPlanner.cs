using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners.DomainEvents;
using LessonFlow.Exceptions;

namespace LessonFlow.Domain.TermPlanners;

public sealed class TermPlanner : Entity<TermPlannerId>, IAggregateRoot
{
    public List<TermPlan> TermPlans { get; private set; } = [];
    public List<YearLevelValue> YearLevels { get; private set; } = [];
    public YearDataId YearDataId { get; private set; }
    public int CalendarYear { get; private set; }

    private static List<YearLevelValue> RemoveDuplicateYearLevels(List<YearLevelValue> yearLevels)
    {
        return yearLevels.Distinct().ToList();
    }

    public void AddYearLevel(YearLevelValue yearLevel)
    {
        if (YearLevels.Contains(yearLevel))
        {
            throw new InputException("Year level already exists");
        }

        YearLevels.Add(yearLevel);
        SortYearLevels();
    }

    public void SortYearLevels()
    {
        if (YearLevels.Count == 1)
        {
            return;
        }

        YearLevels.Sort();
    }

    public void AddTermPlan(TermPlan termPlan)
    {
        if (TermPlans.Contains(termPlan))
        {
            throw new DuplicateTermPlanException();
        }

        if (TermPlans.Count >= 4)
        {
            throw new TooManyTermPlansException();
        }

        if (TermPlans.Any(tp => tp.TermNumber == termPlan.TermNumber))
        {
            throw new DuplicateTermNumberException();
        }

        TermPlans.Add(termPlan);
    }

    public void PopulateSubjectsForTerms(List<Subject> subjects)
    {
        var subjectNumbersForTerms = TermPlans.Select(tp => tp.Subjects.Count)
            .ToArray();

        var subjectCounts = new[] { 0, 0, 0, 0 };

        for (var i = 0; i < subjectNumbersForTerms.Length; i++)
        for (var j = 0; j < subjectNumbersForTerms[i]; j++)
        {
            if (subjectCounts[i] >= subjectNumbersForTerms[i])
            {
                break;
            }

            var subject = subjects.First(s => s.Id == TermPlans[i].Subjects[j].Id);

            if (subject is null)
            {
                continue;
            }

            TermPlans[i].SetSubjectAtIndex(subject, j);
            subjectCounts[i]++;
        }
    }

    public TermPlanner(YearDataId yearDataId, int calendarYear,
        List<YearLevelValue> yearLevels)
    {
        Id = new TermPlannerId(Guid.NewGuid());
        YearDataId = yearDataId;
        CalendarYear = calendarYear;

        YearLevels = RemoveDuplicateYearLevels(yearLevels);
        SortYearLevels();

        AddDomainEvent(new TermPlannerCreatedDomainEvent(Guid.NewGuid(), this, yearDataId));
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TermPlanner()
    {
    }
}