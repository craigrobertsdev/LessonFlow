using LessonFlow.Components.AccountSetup.State;
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
    public string SchoolName { get; private set; } = string.Empty;
    public WeekPlannerTemplate WeekPlannerTemplate { get; set; }
    public Guid UserId { get; init; }
    public TermPlanner? TermPlanner { get; private set; }
    public int CalendarYear { get; init; }
    public List<Student> Students { get; private set; } = [];
    public List<YearLevelValue> YearLevelsTaught { get; private set; } = [];
    public List<Subject> SubjectsTaught { get; private set; } = [];
    public List<LessonPlan> LessonPlans { get; private set; } = [];
    public List<WeekPlanner> WeekPlanners { get; private set; } = [];
    public List<DayOfWeek> WorkingDays { get; private set; } = [];

    public void AddSubjects(List<Subject> subjects)
    {
        foreach (var subject in subjects)
        {
            if (!SubjectsTaught.Contains(subject))
            {
                SubjectsTaught.Add(subject);
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
        if (!Students.Contains(student))
        {
            Students.Add(student);
        }
    }

    public void AddYearLevel(YearLevelValue yearLevel)
    {
        if (!YearLevelsTaught.Contains(yearLevel))
        {
            YearLevelsTaught.Add(yearLevel);
        }
    }

    private bool NotInYearLevelsTaught(YearLevelValue yearLevel)
    {
        return YearLevelsTaught.Contains(yearLevel);
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
        AddDomainEvent(new WeekPlannerTemplateAddedToYearDataEvent(Guid.NewGuid(), WeekPlannerTemplate.Id));
    }

    public void UpdateWeekPlannerTemplate(WeekPlannerTemplate weekPlannerTemplate)
    {
        WeekPlannerTemplate.SetPeriods(weekPlannerTemplate.Periods);
        WeekPlannerTemplate.SetDayTemplates(weekPlannerTemplate.DayTemplates);
    }

    public void SetYearLevelsTaught(List<YearLevelValue> yearLevels)
    {
        yearLevels.Sort();
        YearLevelsTaught.Clear();
        YearLevelsTaught.AddRange(yearLevels);
    }

    public void AddWeekPlanner(WeekPlanner weekPlanner)
    {
        WeekPlanners.Add(weekPlanner);
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

    public YearData(Guid userId, AccountSetupState accountSetupState)
    {
        Id = new YearDataId(Guid.NewGuid());
        UserId = userId;
        SchoolName = accountSetupState.SchoolName;
        CalendarYear = accountSetupState.CalendarYear;
        WeekPlannerTemplate = accountSetupState.WeekPlannerTemplate;
        YearLevelsTaught = accountSetupState.YearLevelsTaught;
        YearLevelsTaught.Sort();
        WorkingDays = accountSetupState.WorkingDays;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private YearData()
    {
    }
}