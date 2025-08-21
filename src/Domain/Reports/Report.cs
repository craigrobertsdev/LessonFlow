using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;

namespace LessonFlow.Domain.Reports;

public sealed class Report : Entity<ReportId>, IAggregateRoot
{
    private readonly List<ReportComment> _reportComments = [];

    public Guid UserId { get; init; }
    public Student Student { get; init; }
    public Subject Subject { get; init; }
    public YearLevelValue YearLevel { get; private set; }
    public IReadOnlyList<ReportComment> ReportComments => _reportComments.AsReadOnly();
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public Report(
        List<ReportComment> reportComments,
        Guid userId,
        Student student,
        Subject subject,
        YearLevelValue yearLevel,
        DateTime createdDateTime,
        DateTime updatedDateTime)
    {
         Id = new ReportId(Guid.NewGuid());
        _reportComments = reportComments;
        UserId = userId;
        Student = student;
        Subject = subject;
        YearLevel = yearLevel;
        CreatedDateTime = createdDateTime;
        UpdatedDateTime = updatedDateTime;
    }


    public void AddReportComment(ReportComment reportComment)
    {
        _reportComments.Add(reportComment);
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Report()
    {
    }
}