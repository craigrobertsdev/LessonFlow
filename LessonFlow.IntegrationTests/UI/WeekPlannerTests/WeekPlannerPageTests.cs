using Bunit;
using LessonFlow.Components.AccountSetup.State;
using LessonFlow.Components.Pages;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Domain.LessonPlans;
using LessonFlow.Domain.PlannerTemplates;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.Users;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Domain.WeekPlanners;
using LessonFlow.Domain.YearDataRecords;
using LessonFlow.Shared.Interfaces.Persistence;
using LessonFlow.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LessonFlow.IntegrationTests.UI.WeekPlannerTests;
public class WeekPlannerPageTests : TestContext
{
}
