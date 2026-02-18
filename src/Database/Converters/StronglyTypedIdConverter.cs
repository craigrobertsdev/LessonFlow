using LessonFlow.Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Database.Converters;

public static class StronglyTypedIdConverter
{
    public class AssessmentIdConverter()
        : ValueConverter<AssessmentId, Guid>(id => id.Value, value => new AssessmentId(value));

    public class CalendarIdConverter()
        : ValueConverter<CalendarId, Guid>(id => id.Value, value => new CalendarId(value));

    public class LessonPlanIdConverter()
        : ValueConverter<LessonPlanId, Guid>(id => id.Value, value => new LessonPlanId(value));

    public class ReportIdConverter() : ValueConverter<ReportId, Guid>(id => id.Value, value => new ReportId(value));

    public class ResourceIdConverter()
        : ValueConverter<ResourceId, Guid>(id => id.Value, value => new ResourceId(value));

    public class SchoolEventIdConverter()
        : ValueConverter<SchoolEventId, Guid>(id => id.Value, value => new SchoolEventId(value));

    public class StudentIdConverter() : ValueConverter<StudentId, Guid>(id => id.Value, value => new StudentId(value));

    public class CurriculumSubjectIdConverter()
        : ValueConverter<SubjectId, Guid>(id => id.Value, value => new SubjectId(value));

    public class TermPlannerIdConverter()
        : ValueConverter<TermPlannerId, Guid>(id => id.Value, value => new TermPlannerId(value));

    public class UserIdConverter() : ValueConverter<UserId, Guid>(id => id.Value, value => new UserId(value));

    public class WeekPlannerIdConverter()
        : ValueConverter<WeekPlannerId, Guid>(id => id.Value, value => new WeekPlannerId(value));

    public class YearPlanIdConverter()
        : ValueConverter<YearPlanId, Guid>(id => id.Value, value => new YearPlanId(value));

    public class FileSystemIdConverter()
        : ValueConverter<FileSystemId, Guid>(id => id.Value, value => new FileSystemId(value));
}