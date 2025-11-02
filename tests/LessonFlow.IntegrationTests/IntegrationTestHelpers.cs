using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Database;
using LessonFlow.Database.Repositories;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Services;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace LessonFlow.IntegrationTests;
internal static class IntegrationTestHelpers
{
    internal readonly static int TestYear = 2025;
    internal readonly static int FirstMonthOfSchool = 1;
    internal readonly static int FirstDayOfSchool = 27;
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
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        List<(string, bool)> userDetails = [("accountsetupnotcomplete@test.com", false), ("test@test.com", true)];
        foreach (var (email, accountSetupComplete) in userDetails)
        {

            var user = new User
            {
                Email = email,
                UserName = email,
                LastSelectedYear = TestYear
            };

            if (accountSetupComplete)
            {
                var accountSetupState = new AccountSetupState(user.Id);
                accountSetupState.SetCalendarYear(TestYear);
                user.AccountSetupState = accountSetupState;
                user.CompleteAccountSetup();
            }

            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            user = dbContext.Users.First(u => u.Email == email);

            var weekPlannerTemplate = IntegrationTestHelpers.GenerateWeekPlannerTemplate(user.Id);
            var yearPlan = new YearPlan(user.Id, weekPlannerTemplate, "Test School", TestYear);
            var weekPlanner = new WeekPlanner(yearPlan.Id, TestYear, 1, 1, FirstDateOfSchool);
            var dayPlan = new DayPlan(weekPlanner.Id, new DateOnly(TestYear, FirstMonthOfSchool, FirstDayOfSchool), [], []);
            weekPlanner.UpdateDayPlan(dayPlan);
            yearPlan.AddWeekPlanner(weekPlanner);
            dbContext.YearPlans.Add(yearPlan);
            dbContext.SaveChanges();

            user.AddYearPlan(yearPlan);

            dbContext.SaveChanges();
        }

        var subjects = new List<Subject>
        {
            new([], "Mathematics"),
            new([], "Science"),
            new([], "English"),
        };

        dbContext.Subjects.AddRange(subjects);
        dbContext.SaveChanges();

        var yearPlans = dbContext.YearPlans.ToList();
        foreach (var yearPlan in yearPlans)
        {
            yearPlan.AddSubjects(subjects);
        }

        var termDatesByYear = new Dictionary<int, List<SchoolTerm>>()
            {
                {
                    2025,
                    [
                        new SchoolTerm(1, new DateOnly(2025, 1, 27), new DateOnly(2025, 4,11)),
                        new SchoolTerm(2, new DateOnly(2025, 4, 28), new DateOnly(2025, 7, 4)),
                        new SchoolTerm(3, new DateOnly(2025, 7, 21), new DateOnly(2025, 9, 26)),
                        new SchoolTerm(4, new DateOnly(2025, 10, 13), new DateOnly(2025, 12, 12))
                    ]
                },
                {
                    2026,
                    [
                        new SchoolTerm(1, new DateOnly(2026, 1, 26), new DateOnly(2026, 4,10)),
                        new SchoolTerm(2, new DateOnly(2026, 4, 27), new DateOnly(2026, 7, 3)),
                        new SchoolTerm(3, new DateOnly(2026, 7, 20), new DateOnly(2026, 9, 25)),
                        new SchoolTerm(4, new DateOnly(2026, 10, 12), new DateOnly(2026, 12, 11))
                    ]
                }
            };

        foreach (var (year, terms) in termDatesByYear)
        {
            dbContext.TermDates.AddRange(terms);
        }

        dbContext.SaveChanges();
    }
}
