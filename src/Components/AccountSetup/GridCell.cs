using LessonFlow.Domain.Enums;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Components.AccountSetup;

public record GridCell
{
    public GridCell(List<(int start, int end)> rowSpans, PeriodBase period, GridColumn column)
    {
        Period = period;
        Column = column;
        RowSpans = rowSpans;
    }
    public GridColumn Column { get; set; } = null!;
    public int StartRow => RowSpans.FirstOrDefault().Start;
    public int EndRow => RowSpans[^1].End;
    public bool IsFirstCellInBlock { get; set; }
    public PeriodBase Period { get; set; }
    public List<(int Start, int End)> RowSpans { get; set; } = [];

    public void SetRowSpans(int oldDuration, int newDuration, List<PeriodBase> templatePeriods)
    {
        if (Period.PeriodType != PeriodType.Lesson)
        {
            throw new InvalidOperationException("Can only set row spans for Lesson periods.");
        }

        if (oldDuration == newDuration) return;

        var rowsCovered = 0;
        var cells = Column.Cells;
        var idx = StartRow - 2;
        var start = StartRow;
        var end = StartRow;
        var breakCount = 0;

        RowSpans.Clear();

        while (rowsCovered < Period.NumberOfPeriods && idx < templatePeriods.Count)
        {
            for (int k = idx; k < templatePeriods.Count; k++)
            {
                // The issue is here. If we are going from a state where there has already been a period removed thus NumberOfPeriods > 1, 
                // The end index will be reduced by that number and affect where the end of the row span is
                // Somehow need to adjust for the 
                if (templatePeriods[k].PeriodType == PeriodType.Lesson)
                {
                    rowsCovered++;

                    if (rowsCovered == Period.NumberOfPeriods)
                    {
                        end = templatePeriods[k].StartPeriod + 2;
                        break;
                    }
                }
                else
                {
                    end = templatePeriods[k].StartPeriod + 1;
                    breakCount++;
                    break;
                }
            }

            RowSpans.Add((start, end));

            var curr = end - 2 + 1; // -2 because col.Cells is 0-indexed and j starts at 2, +1 because we know col.Cells[end-2] is a LessonPeriod
            while (curr < templatePeriods.Count && templatePeriods[curr].PeriodType != PeriodType.Lesson)
            {
                curr++;
                end++;
            }

            idx = curr;
            start = curr + 2;
            end = start;
        }
    }
}