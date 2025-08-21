using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Students;
using LessonFlow.Domain.TermPlanners;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords.DomainEvents;
using LessonFlow.Exceptions;

namespace LessonFlow.Domain.YearDataRecords;

public class YearData : Entity<YearDataId>, IAggregateRoot
{
    private readonly List<LessonPlan> _lessonPlans = [];
    private readonly List<Student> _students = [];
    private readonly List<Subject> _subjectsTaught = [];
    private readonly List<WeekPlanner> _weekPlanners = [];
    private readonly List<YearLevelValue> _yearLevelsTaught = [];
    private readonly List<DayOfWeek> _workingDays = [];

    public string SchoolName { get; private set; } = string.Empty;
    public WeekPlannerTemplate WeekPlannerTemplate { get; set; }
    public Guid UserId { get; init; }
    public TermPlanner? TermPlanner { get; private set; }
    public int CalendarYear { get; init; }
    public IReadOnlyList<Student> Students => _students.AsReadOnly();
    public IReadOnlyList<YearLevelValue> YearLevelsTaught => _yearLevelsTaught.AsReadOnly();
    public IReadOnlyList<Subject> SubjectsTaught => _subjectsTaught.AsReadOnly();
    public IReadOnlyList<LessonPlan> LessonPlans => _lessonPlans.AsReadOnly();
    public IReadOnlyList<WeekPlanner> WeekPlanners => _weekPlanners.AsReadOnly();
    public IReadOnlyList<DayOfWeek> WorkingDays => _workingDays.AsReadOnly();

    public void AddSubjects(List<Subject> subjects)
    {
        foreach (var subject in subjects)
        {
            if (!_subjectsTaught.Contains(subject))
            {
                _subjectsTaught.Add(subject);
            }
        }
    }

    public void AddStudents(List<Student> students)
    {
        foreach (var student in students)
        {
            AddStudent(student);
        }
    }

    public void AddStudent(Student student)
    {
        if (!_students.Contains(student))
        {
            _students.Add(student);
        }
    }

    public void AddYearLevel(YearLevelValue yearLevel)
    {
        if (!_yearLevelsTaught.Contains(yearLevel))
        {
            _yearLevelsTaught.Add(yearLevel);
        }
    }

    private bool NotInYearLevelsTaught(YearLevelValue yearLevel)
    {
        return _yearLevelsTaught.Contains(yearLevel);
    }

    public void AddTermPlanner(TermPlanner termPlanner)
    {
        if (TermPlanner is not null)
        {
            throw new TermPlannerAlreadyAssociatedException();
        }

        TermPlanner = termPlanner;
    }

    public void AddYearLevelsTaught(List<YearLevelValue> yearLevelsTaught)
    {
        foreach (var yearLevel in yearLevelsTaught)
        {
            AddYearLevel(yearLevel);
        }
    }

    public void SetWeekPlannerTemplate(WeekPlannerTemplate weekPlannerTemplate)
    {
        WeekPlannerTemplate = weekPlannerTemplate;
        _domainEvents.Add(new WeekPlannerTemplateAddedToYearDataEvent(Guid.NewGuid(), WeekPlannerTemplate.Id));
    }

    public void UpdateWeekPlannerTemplate(WeekPlannerTemplate weekPlannerTemplate)
    {
        WeekPlannerTemplate.SetPeriods(weekPlannerTemplate.Periods);
        WeekPlannerTemplate.SetDayTemplates(weekPlannerTemplate.DayTemplates);
    }

    public void SetYearLevelsTaught(List<YearLevelValue> yearLevels)
    {
        yearLevels.Sort();
        _yearLevelsTaught.Clear();
        _yearLevelsTaught.AddRange(yearLevels);
    }

    public void AddWeekPlanner(WeekPlanner weekPlanner)
    {
        _weekPlanners.Add(weekPlanner);
    }
    
    public YearData(Guid userId, WeekPlannerTemplate weekPlannerTemplate, string schoolName,
        int calendarYear)
    {
        Id = new YearDataId(Guid.NewGuid());
        UserId = userId;
        SchoolName = schoolName;
        CalendarYear = calendarYear;
        WeekPlannerTemplate = weekPlannerTemplate;
    }

    public YearData(Guid userId, WeekPlannerTemplate weekPlannerTemplate, string schoolName,
        int calendarYear, List<YearLevelValue> yearLevels, List<DayOfWeek> workingDays)
    {
        Id = new YearDataId(Guid.NewGuid());
        UserId = userId;
        SchoolName = schoolName;
        CalendarYear = calendarYear;
        WeekPlannerTemplate = weekPlannerTemplate;
        yearLevels.Sort();
        _yearLevelsTaught = yearLevels;
        _workingDays = workingDays;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private YearData()
    {
    }
}