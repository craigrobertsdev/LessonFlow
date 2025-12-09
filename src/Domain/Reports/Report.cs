using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;

namespace LessonFlow.Domain.Reports;

public sealed class Report : Entity<ReportId>, IAggregateRoot
{
    public Guid UserId { get; init; }
    public Student Student { get; init; }
    public Subject Subject { get; init; }
    public YearLevel YearLevel { get; private set; }
    public List<ReportComment> ReportComments { get; private set; } = [];
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public Report(
        List<ReportComment> reportComments,
        Guid userId,
        Student student,
        Subject subject,
        YearLevel yearLevel,
        DateTime createdDateTime,
        DateTime updatedDateTime)
    {
         Id = new ReportId(Guid.NewGuid());
        ReportComments = reportComments;
        UserId = userId;
        Student = student;
        Subject = subject;
        YearLevel = yearLevel;
        CreatedDateTime = createdDateTime;
        UpdatedDateTime = updatedDateTime;
    }


    public void AddReportComment(ReportComment reportComment)
    {
        ReportComments.Add(reportComment);
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Report()
    {
    }
}