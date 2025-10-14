using LessonFlow.Api.Contracts.Services;
using LessonFlow.Api.Database;
using LessonFlow.Shared.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LessonFlow.Api.Services;

public static class SetTermDates
{
    public static async Task<IResult> Endpoint([FromBody] SetTermDatesRequest request, ApplicationDbContext context,
        ITermDatesService termDatesService, CancellationToken cancellationToken)
    {
        var termDates = context.TermDates.Where(td => td.StartDate.Year == request.TermDates[0].StartDate.Year)
            .ToList();

        if (termDates.Count == 0)
        {
            context.TermDates.AddRange(request.TermDates);
        }
        else
        {
            context.TermDates.RemoveRange(termDates);
            context.TermDates.AddRange(request.TermDates);
        }

        await context.SaveChangesAsync(cancellationToken);

        termDatesService.SetTermDates(request.CalendarYear, request.TermDates);
        return Results.Ok();
    }
}
