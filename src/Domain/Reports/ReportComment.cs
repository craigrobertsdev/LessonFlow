using LessonFlow.Domain.Enums;

namespace LessonFlow.Domain.Reports;

public record ReportComment
{
    public Grade Grade { get; private set; }
    public string Comments { get; private set; }
    public int CharacterLimit { get; private set; }

    public ReportComment(
        Grade grade,
        string comments,
        int characterLimit)
    {
        Grade = grade;
        Comments = comments;
        CharacterLimit = characterLimit;
    }

#pragma warning disable CS8618
    private ReportComment()
    {
    }
}