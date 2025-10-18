using LessonFlow.Components.Shared;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Interfaces.Persistence;
using LessonFlow.Shared;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Components;

namespace LessonFlow.Components.Pages;

public partial class LessonPlanner
{
    [CascadingParameter] public AppState AppState { get; set; } = default!;
    [Parameter] public int Year { get; set; }
    [Parameter] public int Month { get; set; }
    [Parameter] public int Day { get; set; }
    [Parameter] public int StartPeriod { get; set; }

    [Inject] public ILessonPlanRepository LessonPlanRepository { get; set; } = default!;
    [Inject] public ICurriculumService CurriculumService { get; set; } = default!;

    private bool _loading = false;

    internal LessonPlan LessonPlan { get; set; } = default!;
    internal DateOnly Date => new DateOnly(Year, Month, Day);
    internal List<int> AvailableLessonSlots = new List<int> { 1, 2, 3, 4, 5 };
    internal string? SelectedSubject { get; set; }
    internal string? LessonText { get; set; }
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.CurrentYearData.WeekPlannerTemplate;
    internal bool IsInEditMode { get; set; }
    internal LessonPlan? EditingLessonPlan { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AppState is null || AppState.CurrentYearData is null)
        {
            throw new InvalidOperationException("AppState or CurrentYearData is not initialized.");
        }

        try
        {
            _loading = true;
            LessonPlan = await LoadLessonPlan();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading lesson plan: {ex.Message}");
            throw;
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task<LessonPlan> LoadLessonPlan()
    {
        var lessonPlan = await LessonPlanRepository.GetByDateAndPeriodStart(AppState.CurrentYearData.Id, new DateOnly(Year, Month, Day), StartPeriod, new CancellationToken());
        if (lessonPlan is null)
        {
            var templatePeriod = WeekPlannerTemplate.GetTemplatePeriod(Date.DayOfWeek, StartPeriod);
            if (templatePeriod is null)
            {
                lessonPlan = new LessonPlan(
                    AppState.CurrentYearData,
                    new Subject([], CurriculumService.CurriculumSubjects.First().Name),
                    PeriodType.Lesson,
                    "",
                    1,
                    StartPeriod,
                    new DateOnly(Year, Month, Day),
                    []);

                IsInEditMode = true;
            }
            else
            {
                if (templatePeriod is NitPeriod n)
                {
                    lessonPlan = new LessonPlan(
                        AppState.CurrentYearData,
                        Subject.Nit,
                        PeriodType.Nit,
                        "",
                        n.NumberOfPeriods,
                        n.StartPeriod,
                        new DateOnly(Year, Month, Day),
                        []);
                }

                else if (templatePeriod is LessonPeriod p)
                {
                    lessonPlan = new LessonPlan(
                        AppState.CurrentYearData,
                        new Subject([], p.SubjectName),
                        PeriodType.Lesson,
                        "",
                        p.NumberOfPeriods,
                        p.StartPeriod,
                        new DateOnly(Year, Month, Day),
                        []);
                }
                else
                {
                    throw new InvalidOperationException("Trying to create a lesson plan for a non-lesson period.");
                }
            }
        }

        return lessonPlan;
    }
    
    private void EditLessonPlan()
    {
        EditingLessonPlan = LessonPlan.Clone();
        IsInEditMode = true;
    }

    private void CancelEditing()
    {
        IsInEditMode = false;
        EditingLessonPlan = null;
    }

    private void SaveChanges()
    {
        throw new NotImplementedException();
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
