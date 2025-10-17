using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components;

namespace LessonFlow.Components.Pages;

public partial class LessonPlanner
{
    [CascadingParameter] public AppState AppState { get; set; } = default!;
    [Parameter] public int Year { get; set; }
    [Parameter] public int Month { get; set; }
    [Parameter] public int Day { get; set; }
    [Parameter] public int PeriodStart { get; set; }

    [Inject] public ILessonPlanRepository LessonPlanRepository { get; set; } = default!;

    internal LessonPlan LessonPlan { get; set; } = default!;
    internal DateOnly Date => new DateOnly(Year, Month, Day);
    internal List<int> AvailableLessonSlots = new List<int> { 1, 2, 3, 4, 5 };
    internal string? SelectedSubject { get; set; }
    internal string? LessonText { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var lessonPlan = await LessonPlanRepository.GetByDateAndPeriodStart(AppState.CurrentYearData.Id, new DateOnly(Year, Month, Day), PeriodStart, new CancellationToken());
        if (lessonPlan is null)
        {
            var lessonPeriod = AppState.CurrentYearData.WeekPlannerTemplate.DayTemplates.FirstOrDefault(dt => dt.DayOfWeek == Date.DayOfWeek)?
                .Periods.FirstOrDefault(p => p.StartPeriod == PeriodStart);

            if (lessonPeriod is null || lessonPeriod.PeriodType != Domain.Enums.PeriodType.Lesson)
            {
                throw new Exception("No lesson period found for the specified date and period start.");
            }

            lessonPlan = new LessonPlan(
                AppState.CurrentYearData,
                new Subject([], ((LessonPeriod)lessonPeriod).SubjectName),
                PeriodType.Lesson,
                "",
                lessonPeriod.NumberOfPeriods,
                lessonPeriod.StartPeriod,
                new DateOnly(Year, Month, Day),
                []);
        }

        LessonPlan = lessonPlan;
    }
    protected override void OnInitialized()
    {
        SelectedSubject = "math-subject";

        LessonText =
        @"rthsieanrtsheanrths<div>nrt</div><div>seanrt</div><div>seiarnht</div><div>seia</div><div>nreaih</div><div><br></div>";
    }

    void LessonTextChanged(string? text)
    {
        LessonText = text;
    }

    void PrintLessonText()
    {
        Console.WriteLine($"Lesson Text: {LessonText}");
    }
}
