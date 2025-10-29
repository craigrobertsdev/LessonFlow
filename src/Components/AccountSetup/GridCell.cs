using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;
using static LessonFlow.Shared.AppConstants;

namespace LessonFlow.Components.AccountSetup;

public record GridCell
{
    public GridCell(List<(int start, int end)> rowSpans, PeriodTemplateBase period, GridColumn column)
    {
        Period = period;
        Column = column;
        RowSpans = rowSpans;
    }
    public GridColumn Column { get; set; } = null!;
    public int StartRow => RowSpans.FirstOrDefault().Start;
    public int EndRow => RowSpans[^1].End;
    public bool IsHovered { get; set; }
    public bool IsMouseDown { get; set; }
    public bool IsFirstCellInBlock { get; set; }
    public PeriodTemplateBase Period { get; set; }
    public List<(int Start, int End)> RowSpans { get; set; } = [];

    public void SetRowSpans(int oldDuration, int newDuration, List<TemplatePeriod> templatePeriods)
    {
        if (Period.PeriodType == PeriodType.Break)
        {
            throw new InvalidOperationException("Cannot set row spans for Break periods.");
        }

        if (oldDuration == newDuration) return;

        var rowsCovered = 0;
        var idx = StartRow - WEEK_PLANNER_GRID_START_ROW_OFFSET - 1;
        var start = StartRow;
        int end;

        RowSpans.Clear();

        while (rowsCovered < Period.NumberOfPeriods && idx < templatePeriods.Count)
        {
            for (int i = idx; i < templatePeriods.Count; i++)
            {
                if (templatePeriods[i].PeriodType == PeriodType.Lesson)
                {
                    rowsCovered++;
                    if (rowsCovered == Period.NumberOfPeriods)
                    {
                        idx = i;
                        end = templatePeriods[idx].StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET + 1;
                        RowSpans.Add((start, end));
                        return;
                    }
                }
                else
                {
                    idx = i;
                    break;
                }
            }

            end = templatePeriods[idx].StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET;
            RowSpans.Add((start, end));

            idx++;
            while (templatePeriods[idx].PeriodType == PeriodType.Break)
            {
                idx++;
            }

            start = templatePeriods[idx].StartPeriod + WEEK_PLANNER_GRID_START_ROW_OFFSET;
        }
    }
}