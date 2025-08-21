using LessonFlow.Domain.Common.Interfaces;
using LessonFlow.Domain.Common.Primatives;
using LessonFlow.Domain.StronglyTypedIds;
using LessonFlow.Domain.ValueObjects;

namespace LessonFlow.Domain.Calendar;

// TODO: fix the shit ambiguous name of this class

// The calendar provides data for a high level overview of what the school year looks like
// It will show in a calendar format, anything of interest for each day outside of the usual lesson planning
// Things like events (sports day, camp, aquatics), report due dates, parent-teacher interviews etc.

public sealed class Calendar : Entity<CalendarId>, IAggregateRoot
{
    private readonly List<SchoolEvent> _schoolEvents = [];

    public int TermNumber { get; private set; }
    public DateTime TermStart { get; private set; }
    public DateTime TermEnd { get; private set; }
    public IReadOnlyList<SchoolEvent> SchoolEvents => _schoolEvents.AsReadOnly();
    public DateTime CreatedDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }

    public Calendar(
        List<SchoolEvent>? schoolEvents,
        int termNumber,
        DateTime termStart,
        DateTime termEnd,
        DateTime createdDateTime,
        DateTime updatedDateTime) 
    {
        if (schoolEvents is not null)
        {
            _schoolEvents = schoolEvents;
        }

        Id = new CalendarId(Guid.NewGuid());
        TermNumber = termNumber;
        TermStart = termStart;
        TermEnd = termEnd;
        CreatedDateTime = createdDateTime;
        UpdatedDateTime = updatedDateTime;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Calendar()
    {
    }
}