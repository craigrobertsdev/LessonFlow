using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Api.Contracts.Services;

public record SetTermDatesRequest(int CalendarYear, List<SchoolTerm> TermDates);