using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.TermPlanners.DomainEvents;
using LessonFlow.Shared.Exceptions;

namespace LessonFlow.Domain.TermPlanners;

public sealed class TermPlanner : Entity<TermPlannerId>, IAggregateRoot
{
    public List<TermPlan> TermPlans { get; private set; } = [];
    public List<YearLevel> YearLevels { get; private set; } = [];
    public YearPlanId YearPlanId { get; private set; }
    public int CalendarYear { get; private set; }
    //public Dictionary<YearLevel, List<ConceptualOrganiser>> PlannedConceptualOrganisers { get; private set; } = [];

    private static List<YearLevel> RemoveDuplicateYearLevels(List<YearLevel> yearLevels)
    {
        return yearLevels.Distinct().ToList();
    }

    public void AddYearLevel(YearLevel yearLevel)
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

    public TermPlanner(YearPlanId yearPlanId, int calendarYear,
        List<YearLevel> yearLevels)
    {
        Id = new TermPlannerId(Guid.NewGuid());
        YearPlanId = yearPlanId;
        CalendarYear = calendarYear;

        YearLevels = RemoveDuplicateYearLevels(yearLevels);
        SortYearLevels();

        for (var termNumber = 1; termNumber <= 4; termNumber++)
        {
            TermPlans.Add(new TermPlan(this, termNumber, []));
        }

        AddDomainEvent(new TermPlannerCreatedDomainEvent(Guid.NewGuid(), this, yearPlanId));
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TermPlanner()
    {
    }
}