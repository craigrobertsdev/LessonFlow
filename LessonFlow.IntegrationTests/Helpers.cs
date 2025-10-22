using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Database;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;

namespace LessonFlow.IntegrationTests;
internal static class Helpers
{
    internal readonly static int TestYear = 2025;
    internal readonly static int FirstMonthOfSchool = 1;
    internal readonly static int FirstDayOfSchool = 29;
    internal readonly static DateOnly FirstDateOfSchool = new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool);

    internal static WeekPlannerTemplate GenerateWeekPlannerTemplate(Guid userId)
    {
        var periods = new List<TemplatePeriod>
        {
            new TemplatePeriod(PeriodType.Lesson, 1, "Lesson 1", new TimeOnly(09, 10, 0), new TimeOnly(10, 00, 0)),
            new TemplatePeriod(PeriodType.Lesson, 2, "Lesson 2", new TimeOnly(10, 00, 0), new TimeOnly(10, 50, 0)),
            new TemplatePeriod(PeriodType.Break, 3, "Recess", new TimeOnly(10, 50, 0), new TimeOnly(11, 20, 0)),
            new TemplatePeriod(PeriodType.Lesson, 4, "Lesson 3", new TimeOnly(11, 20, 0), new TimeOnly(12, 10, 0)),
            new TemplatePeriod(PeriodType.Lesson, 5, "Lesson 4", new TimeOnly(12, 10, 0), new TimeOnly(13, 00, 0)),
            new TemplatePeriod(PeriodType.Break, 6, "Lunch", new TimeOnly(13, 0, 0), new TimeOnly(13, 30, 0)),
            new TemplatePeriod(PeriodType.Lesson,7, "Lesson 5", new TimeOnly(13, 30, 0), new TimeOnly(14, 20, 0)),
            new TemplatePeriod(PeriodType.Lesson, 8,"Lesson 6", new TimeOnly(14, 20, 0), new TimeOnly(15, 10, 0))
        };
        var dayTemplates = new List<DayTemplate>();
        foreach (var day in Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday))
        {
            dayTemplates.Add(new DayTemplate(periods.Select<TemplatePeriod, PeriodTemplateBase>((p, i) =>
            {
                if (p.PeriodType == PeriodType.Lesson)
                {
                    return new LessonTemplate(p.Name ?? string.Empty, i + 1, 1);
                }
                else
                {
                    return new BreakTemplate(p.Name ?? string.Empty, i + 1, 1);
                }
            }).ToList(), day, DayType.Working));
        }
        var template = new WeekPlannerTemplate(userId, periods, dayTemplates);
        return template;
    }

    internal static void SeedDbContext(ApplicationDbContext dbContext)
    {
        var accountSetupState = new AccountSetupState(Guid.NewGuid());
        accountSetupState.SetCalendarYear(TestYear);

        var user = new User
        {
            AccountSetupState = accountSetupState,
            Email = "test@test.com",
            UserName = "testuser",
            LastSelectedYear = TestYear
        };
        user.CompleteAccountSetup();
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        user = dbContext.Users.First();

        var weekPlannerTemplate = Helpers.GenerateWeekPlannerTemplate(user.Id);
        var yearData = new YearData(user.Id, weekPlannerTemplate, "Test School", TestYear);
        var weekPlanner = new WeekPlanner(yearData, TestYear, 1, 1, FirstDateOfSchool);
        var dayPlan = new DayPlan(weekPlanner.Id, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool), [], []);
        weekPlanner.UpdateDayPlan(dayPlan);
        yearData.AddWeekPlanner(weekPlanner);
        dbContext.YearData.Add(yearData);
        dbContext.SaveChanges();

        user.AddYearData(yearData);
        dbContext.SaveChanges();

        var subjects = new List<Subject>
        {
            new([], "Mathematics"),
            new([], "Science"),
            new([], "English"),
        };

        dbContext.Subjects.AddRange(subjects);
        dbContext.SaveChanges();

        yearData.AddSubjects(subjects);
        dbContext.SaveChanges();
    }
}
