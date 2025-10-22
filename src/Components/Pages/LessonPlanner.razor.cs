using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Shared;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Interfaces.Persistence;
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
    [Inject] public IYearDataRepository YearDataRepository { get; set; } = default!;
    [Inject] public ISubjectRepository SubjectRepository { get; set; } = default!;
    [Inject] public IUnitOfWork UnitOfWork { get; set; } = default!;

    private bool _loading = false;

    internal LessonPlan LessonPlan { get; set; } = default!;
    internal DateOnly Date => new(Year, Month, Day);
    internal List<int> AvailableLessonSlots { get; set; } = [];
    internal string? SelectedSubject { get; set; }
    internal string? LessonText { get; set; }
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.CurrentYearData.WeekPlannerTemplate;
    internal DayPlan DayPlan => AppState.CurrentYearData.GetDayPlan(Date);
    internal List<Subject> SubjectsTaught => AppState.CurrentYearData.SubjectsTaught;
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
            AvailableLessonSlots = GetAvailableLessonSlots();
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

                EditingLessonPlan = lessonPlan.Clone();

                IsInEditMode = true;
            }
            else
            {
                if (templatePeriod is NitTemplatePeriod n)
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

                else if (templatePeriod is LessonTemplate p)
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

    private List<int> GetAvailableLessonSlots()
    {
        var periodsToEndOfDay = WeekPlannerTemplate.GetLessonPeriodCount(StartPeriod);
        return [.. Enumerable.Range(1, periodsToEndOfDay)];
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

    private async Task SaveChanges()
    {
        if (EditingLessonPlan is null) return;
        var cancellationToken = new CancellationToken();

        LessonPlan.UpdateValuesFrom(EditingLessonPlan);

        UnitOfWork.BeginTransaction();

        var lessonPlanExists = LessonPlanRepository.UpdateLessonPlan(LessonPlan);
        if (!lessonPlanExists)
        {
            var subject = await SubjectRepository.GetSubjectById(LessonPlan.Subject.Id, cancellationToken);
            if (subject is null)
            {
                throw new SubjectNotFoundException(LessonPlan.Subject.Name);
            }

            LessonPlan.UpdateSubject(subject);
            DayPlan.AddLessonPlan(LessonPlan);
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            LessonPlanRepository.Add(LessonPlan);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        await UnitOfWork.CommitTransaction(cancellationToken);

        IsInEditMode = false;
        EditingLessonPlan = null;
    }

    void LessonTextChanged(string? text)
    {
        LessonText = text;
    }

    void PrintLessonText()
    {
        Console.WriteLine($"Lesson Text: {LessonText}");
    }

    private void HandleSubjectTaughtChanged(ChangeEventArgs args)
    {
        if (EditingLessonPlan is null) return;

        var subject = CurriculumService.GetSubjectByName(args.Value!.ToString()!);
        if (subject != null)
        {
            EditingLessonPlan.UpdateSubject(subject);
        }
    }

    private void HandleLessonDurationChange(ChangeEventArgs args)
    {
        if (EditingLessonPlan is null) return;
        if (int.TryParse(args.Value?.ToString(), out var duration))
        {
            EditingLessonPlan.SetNumberOfPeriods(duration);
        }
    }
}
