using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.Users;

namespace LessonFlow.Domain.Assessments;

public class Assessment : Entity<AssessmentId>, IAggregateRoot
{
    public Assessment(
        User user,
        Subject subject,
        Student student,
        YearLevel yearLevel,
        string planningNotes,
        DateTime conductedDateTime)
    {
        Id = new AssessmentId(Guid.NewGuid());
        User = user;
        Subject = subject;
        Student = student;
        YearLevel = yearLevel;
        PlanningNotes = planningNotes;
        DateConducted = conductedDateTime;
    }

    public User User { get; private set; }
    public Subject Subject { get; private set; }
    public Student Student { get; private set; }
    public YearLevel YearLevel { get; private set; }
    public AssessmentType AssessmentType { get; }
    public AssessmentResult? AssessmentResult { get; private set; }
    public string PlanningNotes { get; private set; }
    public DateTime DateConducted { get; private set; }
    public DateTime CreatedDateTime { get; }
    public DateTime UpdatedDateTime { get; private set; }

    public void SetAssessmentResult(AssessmentResult result)
    {
        if (AssessmentResult is not null)
        {
            AssessmentResult = result;
            UpdatedDateTime = DateTime.Now;
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Assessment()
    {
    }
}