using LessonFlow.Database;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Shared.Interfaces.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LessonFlow.IntegrationTests.IntegrationTestHelpers;

namespace LessonFlow.IntegrationTests.Repositories;
[Collection("Non-Parallel")]
public class LessonPlanRepositoryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly ApplicationDbContext _dbContext;
    public LessonPlanRepositoryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SeedDbContext(_dbContext);
    }

    [Fact]
    public async Task GetConflictingLessonPlans_WhenNoConflictAndNoLessonsPlanned_ShouldReturnEmptyList()
    {
        var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>();
        var dayPlanId = new DayPlanId(Guid.NewGuid());
        var lessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            1,
            2,
            FirstDateOfSchool,
            []
        );

        var conflicts = await repository.GetConflictingLessonPlans(dayPlanId, lessonPlan, CancellationToken.None);

        Assert.Empty(conflicts);
    }

    [Fact]
    public async Task GetConflictingLessonPlans_WhenNoConflictAndLessonsPlannedOutsideOfConflictRange_ShouldReturnEmptyList()
    {
        var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dayPlanId = dbContext.WeekPlanners.First().DayPlans.First().Id;
        var lessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            1,
            3,
            FirstDateOfSchool,
            []
        );

        dbContext.LessonPlans.Add(lessonPlan);
        dbContext.SaveChanges();

        var nonConflictingLessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            2,
            1,
            FirstDateOfSchool,
            []
        );

        var conflicts = await repository.GetConflictingLessonPlans(dayPlanId, lessonPlan, CancellationToken.None);

        Assert.Empty(conflicts);
    }

    [Fact]
    public async Task GetConflictingLessonPlans_WhenOneConflictingLessonExists_ShouldReturnThatLessonPlan()
    {
        var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dayPlanId = dbContext.WeekPlanners.First().DayPlans.First().Id;
        var conflictingLessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            1,
            2,
            FirstDateOfSchool,
            []
        );

        dbContext.LessonPlans.Add(conflictingLessonPlan);
        dbContext.SaveChanges();

        var lessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            2,
            1,
            FirstDateOfSchool,
            []
        );

        var conflicts = await repository.GetConflictingLessonPlans(dayPlanId, lessonPlan, CancellationToken.None);

        Assert.Single(conflicts);
        Assert.Equal(conflicts[0].Id, conflictingLessonPlan.Id);
    }

    [Fact]
    public async Task GetConflictingLessonPlans_WhenMultipleConflictingLessonExists_ShouldReturnThoseLessonPlan()
    {
        var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILessonPlanRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dayPlanId = dbContext.WeekPlanners.First().DayPlans.First().Id;
        var conflictingLessonPlanIds = new List<LessonPlanId>();
        for (int i = 0; i < 3; i++)
        {
            var conflictingLessonPlan = new LessonPlan(
                dayPlanId,
                new Subject("Mathematics", [], ""),
                PeriodType.Lesson,
                "",
                1,
                2 + i,
                FirstDateOfSchool,
                []
            );
            dbContext.LessonPlans.Add(conflictingLessonPlan);
            conflictingLessonPlanIds.Add(conflictingLessonPlan.Id);
        }

        dbContext.SaveChanges();

        var lessonPlan = new LessonPlan(
            dayPlanId,
            new Subject("Mathematics", [], ""),
            PeriodType.Lesson,
            "",
            4,
            1,
            FirstDateOfSchool,
            []
        );

        var conflicts = await repository.GetConflictingLessonPlans(dayPlanId, lessonPlan, CancellationToken.None);

        Assert.Equal(3, conflicts.Count);
        foreach (var conflictingLessonPlanId in conflictingLessonPlanIds)
        {
            Assert.Contains(conflicts, lp => lp.Id == conflictingLessonPlanId);
        }
    }
}
