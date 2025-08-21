using LessonFlow.Domain.Assessments;
using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Reports;
using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Domain.Students;

public sealed class Student : Entity<StudentId>, IAggregateRoot
{
    private readonly List<Assessment> _assessments = [];
    private readonly List<Report> _reports = [];

    public Guid UserId { get; init; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }


    public IReadOnlyList<Report> Reports => _reports;
    public IReadOnlyList<Assessment> Assessments => _assessments;

    public Student(
        Guid userId,
        string firstName,
        string lastName)
    {
        Id = new StudentId(Guid.NewGuid());
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Student()
    {
    }
}