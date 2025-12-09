using LessonFlow.Domain.Curriculum;
using LessonFlow.Shared.Exceptions;

namespace LessonFlow.Domain.TermPlanners;

public record TermPlan
{
    public TermPlan(TermPlanner termPlanner, int termNumber, List<Subject> subjects)
    {
        TermPlanner = termPlanner;
        TermNumber = termNumber;
        Subjects = subjects;
    }

    public List<Subject> Subjects { get; private set; } = [];
    public TermPlanner TermPlanner { get; private set; } = null!;
    public int TermNumber { get; private set; }

    public void AddSubject(Subject subject)
    {
        if (!Subjects.Contains(subject))
        {
            Subjects.Add(subject);
        }
    }

    public void AddSubjects(List<Subject> subjects)
    {
        if (Subjects.Count > 0)
        {
            throw new TermPlanSubjectsAlreadySetException();
        }

        Subjects.AddRange(subjects);
    }

    public void SetSubjectAtIndex(Subject subject, int index)
    {
        Subjects[index] = subject;
    }

    public void UpdateSubject(Subject subject)
    {
        var subjectToUpdate = Subjects.FirstOrDefault(s => s.Id == subject.Id);

        if (subjectToUpdate is null)
        {
            Subjects.Add(subject);
            return;
        }

        // Find the difference between the two subjects and add the new content descriptions
        var yearLevel = subject.YearLevels[0];
        var yearLevelToUpdate = subjectToUpdate.YearLevels.FirstOrDefault(yl => yl.Name == yearLevel.Name);

        if (yearLevelToUpdate is null)
        {
            subject.AddYearLevel(yearLevel);
        }
        // TODO:
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TermPlan()
    {
    }
}