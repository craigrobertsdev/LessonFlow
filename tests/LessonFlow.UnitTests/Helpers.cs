using LessonFlow.Services;
using LessonFlow.Components.AccountSetup;
using LessonFlow.Database;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using LessonFlow.Domain.Curriculum;

namespace LessonFlow.UnitTests;
internal class Helpers
{
    public static GridColumn GenerateGridColumn()
    {
        var col = new GridColumn(1)
        {
            IsWorkingDay = true
        };

        var periods = new List<PeriodBase>
        {
            new LessonPeriod("", 1, 1),
            new LessonPeriod("", 2, 1),
            new BreakPeriod("Recess", 3, 1),
            new LessonPeriod("", 4, 1),
            new LessonPeriod("", 5, 1),
            new BreakPeriod("Lunch", 6, 1),
            new LessonPeriod("", 7, 1),
            new LessonPeriod("", 8, 1)
        };

        foreach (var period in periods)
        {
            var cell = new GridCell([], period, col);
            cell.RowSpans.Add((period.StartPeriod + 2, period.StartPeriod + 3));
            cell.IsFirstCellInBlock = true;
            col.Cells.Add(cell);
        }

        return col;
    }

    internal static WeekPlannerTemplate GenerateWeekPlannerTemplate()
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
            dayTemplates.Add(new DayTemplate(periods.Select<TemplatePeriod, PeriodBase>((p, i) =>
            {
                if (p.PeriodType == PeriodType.Lesson)
                {
                    return new LessonPeriod(p.Name ?? string.Empty, i + 1, 1);
                }
                else
                {
                    return new BreakPeriod(p.Name ?? string.Empty, i + 1, 1);
                }
            }).ToList(), day, DayType.Working));
        }
        var template = new WeekPlannerTemplate(periods, dayTemplates, Guid.NewGuid());
        return template;
    }

    public static ITermDatesService CreateTermDatesService()
    {

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

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new Mock<ApplicationDbContext>(dbContextOptions);
        var allYears = termDatesByYear.Values.SelectMany(v => v).ToList();

        var mockDbSet = new Mock<DbSet<SchoolTerm>>();
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.Provider).Returns(allYears.AsQueryable().Provider);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.Expression).Returns(allYears.AsQueryable().Expression);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.ElementType).Returns(allYears.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.GetEnumerator()).Returns(allYears.AsQueryable().GetEnumerator());

        dbContext.Setup(db => db.TermDates).Returns(mockDbSet.Object);

        var mockScope = new Mock<IServiceScope>();
        var mockScopedServiceProvider = new Mock<IServiceProvider>();

        mockScopedServiceProvider
            .Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
            .Returns(dbContext.Object);

        mockScope
            .Setup(s => s.ServiceProvider)
            .Returns(mockScopedServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(mockScope.Object);

        var mockRootServiceProvider = new Mock<IServiceProvider>();
        mockRootServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var service = new TermDatesService(mockRootServiceProvider.Object);

        foreach (var (year, terms) in termDatesByYear)
        {
            service.SetTermDates(year, terms);
        }
        return service;
    }

    internal static ICurriculumService CreateCurriculumService()
    {
        var curriculumService = new Mock<ICurriculumService>();
        var subjects = new List<Subject>
        {
            new Subject("English", [], ""),
            new Subject("Mathematics", [], ""),
            new Subject("Science", [], "")
        };
        curriculumService.Setup(s => s.CurriculumSubjects).Returns(subjects);

        foreach (var subject in subjects)
        {
            curriculumService.Setup(s => s.GetSubjectByName(subject.Name)).Returns(subject);
        }

        return curriculumService.Object;
    }
}
