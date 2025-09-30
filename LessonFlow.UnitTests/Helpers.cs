using LessonFlow.Components.AccountSetup;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;

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
            cell.RowSpans.Add((period.StartPeriod + 1, period.StartPeriod + 2));
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
            }).ToList(), day, DayType.WorkingDay));
        }
        var template = new WeekPlannerTemplate(periods, dayTemplates, Guid.NewGuid());
        return template;
    }
}
