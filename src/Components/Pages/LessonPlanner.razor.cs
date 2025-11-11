using LessonFlow.Components.Shared;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.YearPlans;
using LessonFlow.Shared;
using LessonFlow.Shared.Exceptions;
using LessonFlow.Shared.Extensions;
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
    [Inject] public IYearPlanRepository YearPlanRepository { get; set; } = default!;
    [Inject] public ISubjectRepository SubjectRepository { get; set; } = default!;
    [Inject] public ITermDatesService TermDatesService { get; set; } = default!;
    [Inject] public IUnitOfWorkFactory UnitOfWorkFactory { get; set; } = default!;

    private bool _loading = true;
    private bool _dayPlanFromDb;
    private bool _cancelSaveOperation;

    internal LessonPlan LessonPlan { get; set; } = default!;
    internal DateOnly Date { get; set; }
    internal List<int> AvailableLessonSlots { get; set; } = [];
    internal string? SelectedSubject { get; set; }
    internal string? LessonText { get; set; }
    internal WeekPlannerTemplate WeekPlannerTemplate => AppState.CurrentYearPlan.WeekPlannerTemplate;
    internal DayPlan DayPlan { get; set; } = default!;
    internal List<Subject> SubjectsTaught => AppState.CurrentYearPlan.SubjectsTaught;
    internal bool IsInEditMode { get; set; }
    internal LessonPlan? EditingLessonPlan { get; set; }
    internal ConfirmationDialog? OverwriteConfirmationDialog { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AppState is null || AppState.CurrentYearPlan is null)
        {
            throw new InvalidOperationException("AppState or CurrentYearPlan is not initialized.");
        }

        _loading = true;
        try
        {
            Date = new DateOnly(Year, Month, Day);
            var ct = new CancellationToken();
            var weekStart = Date.GetWeekStart();

            var dbWeekPlanner = await YearPlanRepository.GetWeekPlanner(AppState.CurrentYearPlan.Id, weekStart, ct);

            WeekPlanner weekPlanner;
            DayPlan? dayPlan;

            if (dbWeekPlanner is not null)
            {
                weekPlanner = AppState.CurrentYearPlan.GetWeekPlanner(weekStart) ?? dbWeekPlanner;

                dayPlan = weekPlanner.GetDayPlan(Date);
                if (dayPlan is null)
                {
                    var dbDayPlan = dbWeekPlanner.DayPlans.FirstOrDefault(dp => dp.Date == Date);
                    if (dbDayPlan is not null)
                    {
                        weekPlanner.UpdateDayPlan(dbDayPlan);
                        dayPlan = dbDayPlan;
                        _dayPlanFromDb = true;
                    }
                }
                else
                {
                    _dayPlanFromDb = true;
                }

                if (dayPlan is null)
                {
                    dayPlan = new DayPlan(weekPlanner.Id, Date, [], []);
                    weekPlanner.UpdateDayPlan(dayPlan);
                    _dayPlanFromDb = false;
                }
            }
            else
            {
                weekPlanner = AppState.CurrentYearPlan.GetWeekPlanner(weekStart)
                    ?? new WeekPlanner(
                        AppState.CurrentYearPlan.Id,
                        Year,
                        TermDatesService.GetTermNumber(Date),
                        TermDatesService.GetWeekNumber(Date),
                        Date.GetWeekStart());

                if (AppState.CurrentYearPlan.GetWeekPlanner(weekStart) is null)
                {
                    AppState.CurrentYearPlan.AddWeekPlanner(weekPlanner);
                }
                dayPlan = weekPlanner.GetDayPlan(Date);
                if (dayPlan is null)
                {
                    dayPlan = new DayPlan(weekPlanner.Id, Date, [], []);
                    weekPlanner.UpdateDayPlan(dayPlan);
                }
                _dayPlanFromDb = false;
            }

            DayPlan = dayPlan;

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
        var lessonPlan = DayPlan.LessonPlans.FirstOrDefault(lp => lp.StartPeriod == StartPeriod);
        if (lessonPlan is not null)
        {
            return lessonPlan;
        }

        if (_dayPlanFromDb)
        {
            var found = await LessonPlanRepository.GetLessonPlan(DayPlan.Id, new DateOnly(Year, Month, Day), StartPeriod, new CancellationToken());

            if (found is not null)
            {
                DayPlan.AddLessonPlan(found);
                return found;
            }
        }

        var templatePeriod = WeekPlannerTemplate.GetTemplatePeriod(Date.DayOfWeek, StartPeriod);
        if (templatePeriod is null)
        {
            lessonPlan = new LessonPlan(
                DayPlan.Id,
                SubjectsTaught.First(),
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
            if (templatePeriod is NitTemplate n)
            {
                lessonPlan = new LessonPlan(
                    DayPlan.Id,
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
                    DayPlan.Id,
                    SubjectsTaught.First(),
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

        DayPlan.AddLessonPlan(lessonPlan);
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

        var ct = new CancellationToken();
        await CheckLessonPlanOverwrite(ct);
        if (_cancelSaveOperation) return;

        LessonPlan.UpdateValuesFrom(EditingLessonPlan);

        await using var uow = UnitOfWorkFactory.Create();

        var trackedWeekPlanner = await YearPlanRepository.GetOrCreateWeekPlanner(
            AppState.CurrentYearPlan.Id,
            Year,
            TermDatesService.GetTermNumber(Date),
            TermDatesService.GetWeekNumber(Date),
            Date.GetWeekStart(),
            ct);

        var trackedDayPlan = trackedWeekPlanner.GetDayPlan(Date);
        if (trackedDayPlan is null)
        {
            trackedDayPlan = new DayPlan(trackedWeekPlanner.Id, Date, [], []);
            trackedWeekPlanner.UpdateDayPlan(trackedDayPlan);
        }

        await uow.SaveChangesAsync(ct);

        var lessonPlanExists = await LessonPlanRepository.UpdateLessonPlan(LessonPlan, ct);
        if (!lessonPlanExists)
        {
            var subject = CurriculumService.GetSubjectByName(LessonPlan.Subject.Name)
                ?? throw new SubjectNotFoundException(LessonPlan.Subject.Name);

            LessonPlan.UpdateSubject(subject);

            trackedDayPlan.AddLessonPlan(LessonPlan);
            LessonPlanRepository.Add(LessonPlan);
        }

        trackedWeekPlanner.UpdateDayPlan(trackedDayPlan);

        await uow.SaveChangesAsync(ct);

        var stateWeekPlanner = AppState.CurrentYearPlan.GetWeekPlanner(Date.GetWeekStart());
        stateWeekPlanner?.UpdateDayPlan(trackedDayPlan);

        IsInEditMode = false;
        EditingLessonPlan = null;
    }

    private async Task CheckLessonPlanOverwrite(CancellationToken ct)
    {
        if (EditingLessonPlan is null || EditingLessonPlan.NumberOfPeriods == 1) return;

        var conflictingLessonPlans = await LessonPlanRepository.GetConflictingLessonPlans(DayPlan.Id, EditingLessonPlan, ct);
        if (conflictingLessonPlans.Count == 0) return;

        await OverwriteConfirmationDialog!.Open();
    }

    private async Task OpenModal()
    {
        await OverwriteConfirmationDialog.Open();
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

        var subjectName = args.Value?.ToString();
        var subject = SubjectsTaught.FirstOrDefault(s => s.Name == subjectName);
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
