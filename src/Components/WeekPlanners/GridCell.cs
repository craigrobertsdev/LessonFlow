using LessonFlow.Domain.Enums;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Shared.Interfaces;

namespace LessonFlow.Components.WeekPlanners;

public record GridCell
{
    public GridCell(List<(int start, int end)> rowSpans, IPlannerPeriod period, GridColumn column, int periodNumber)
    {
        Period = period;
        Column = column;
        RowSpans = rowSpans;
        PeriodNumber = periodNumber;
    }

    public LessonPlanId LessonPlanId { get; set; } = default!;
    public GridColumn Column { get; set; }
    public IPlannerPeriod Period { get; set; }
    public int PeriodNumber { get; set; }
    public int StartRow => RowSpans.FirstOrDefault().Start;
    public int EndRow => RowSpans[^1].End;
    public bool IsHovered { get; set; }
    public bool IsMouseDown { get; set; }
    public bool IsFirstCellInBlock { get; set; }

    public List<(int Start, int End)> RowSpans { get; set; } = [];

    public void SetRowSpans(int oldDuration, int newDuration, List<TemplatePeriod> templatePeriods)
    {
        if (Period.PeriodType == PeriodType.Break)
        {
            throw new InvalidOperationException("Cannot set row spans for Break periods.");
        }

        if (oldDuration == newDuration) return;

        var rowsCovered = 0;
        var idx = StartRow - 2;
        var breaksCovered = 0;
        var start = StartRow + breaksCovered;
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
                        end = templatePeriods[idx].StartPeriod + 2;
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

            end = templatePeriods[idx].StartPeriod + 1;
            RowSpans.Add((start, end));

            idx++;
            while (templatePeriods[idx].PeriodType == PeriodType.Break)
            {
                idx++;
            }

            start = templatePeriods[idx].StartPeriod + 1;
        }
    }
}
