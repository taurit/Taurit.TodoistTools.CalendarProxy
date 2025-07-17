using EWSoftware.PDI.Objects;
using EWSoftware.PDI.Parser;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers;

/// <summary>
///     Allows to filter calendar events according to user-defined rules
/// </summary>
public class EventManager
{
    /// <summary>
    ///     Backing field containing main calendar object
    /// </summary>
    private readonly VCalendarParser calendar;

    /// <summary>
    ///     Constructor. Parses given VCalendar string and stores internally as VCalendar object.
    /// </summary>
    public EventManager(string icalContent)
    {
        calendar = new VCalendarParser();
        calendar.ParseString(icalContent);
    }

    /// <summary>
    ///     Alternative constructor that initialises object based on given calendar object, without parsing anything
    /// </summary>
    public EventManager(VCalendarParser calendar)
    {
        this.calendar = calendar;
    }

    /// <summary>
    ///     Filter events in loaded VCalendar based on provided filtering options
    /// </summary>
    public void Filter(FilteringOptions filter)
    {
        if (filter.PredictEventDuration)
            PredictEventDuration(filter.RemoveEventDurationFromTitle);

        if (filter.HideAllDayEvents)
            HideAllDayEvents(); // should be called before shortening events
        if (filter.HidePrivateEvents)
            HidePrivateEvents();

        // New filter to remove Teams locations
        if (filter.RemoveTeamsLocations)
            RemoveTeamsLocations();

        IList<string> projectsToSkip = filter.ProjectsToSkip;
        if (projectsToSkip.Count > 0) SkipEventsFromProjects(projectsToSkip);

        if (filter.ShortenEvents)
            ShortenEvents();

        if (!string.IsNullOrEmpty(filter.HideEventsContainingThisString))
            HideEventsContainingString(filter.HideEventsContainingThisString);

        if (filter.ShortenEventsLongerThanThisMinutes > 0 && filter.ShortenEventsLongerThanToThisMinutes > 0)
            ShortenEvents(filter.ShortenEventsLongerThanThisMinutes, filter.ShortenEventsLongerThanToThisMinutes);

        if (filter.HideEventsShorterThanMinutes > 0)
            HideEventsShorterThanMinutes(filter.HideEventsShorterThanMinutes);
    }

    /// <summary>
    ///     Returns calendar in iCalendar format, that might be returned to iCal-compatible organizer programs
    /// </summary>
    public string GetIcal()
    {
        return calendar.VCalendar.ToString();
    }

    /// <summary>
    ///     Returns all events in current calendar as a generic list
    /// </summary>
    public IList<VEvent> GetEventList()
    {
        return calendar.VCalendar.Events.OfType<VEvent>().ToList();
    }

    /// <summary>
    ///     Removes from calendar all events that start at midnight and last 24 hours
    /// </summary>
    private void HideAllDayEvents()
    {
        List<VEvent> eventsToRemove = new List<VEvent>();
        foreach (VEvent evnt in calendar.VCalendar.Events.Where(evnt => evnt.IsAllDayEvent()))
            eventsToRemove.Add(evnt);

        foreach (VEvent evnt in eventsToRemove)
            calendar.VCalendar.Events.Remove(evnt);
    }

    private void HidePrivateEvents()
    {
        List<VEvent> eventsToRemove = new List<VEvent>();
        foreach (VEvent evnt in calendar.VCalendar.Events.Where(evnt => evnt.IsPrivate()))
        {
            eventsToRemove.Add(evnt);
        }

        foreach (VEvent evnt in eventsToRemove)
            calendar.VCalendar.Events.Remove(evnt);
    }

    /// <summary>
    ///     Try to parse event's title to find indication of how long is the event. This method does not process all-day
    ///     events.
    ///     If such information is found and considered reliable, event's EndDate is set based on StartDate and found event
    ///     duration.
    /// </summary>
    private void PredictEventDuration(bool removeEventDurationFromTitle)
    {
        foreach (VEvent evnt in calendar.VCalendar.Events)
        {
            if (evnt.IsAllDayEvent()) continue;
            EventLengthFinder elf = new EventLengthFinder(evnt.Summary.Value);
            if (elf.PatternFound)
            {
                evnt.EndDateTime.DateTimeValue = evnt.StartDateTime.DateTimeValue.AddMinutes(elf.TotalMinutes);
                if (removeEventDurationFromTitle) evnt.Summary.Value = elf.TaskSummaryWithoutPattern;
            }
        }
    }

    /// <summary>
    ///     Shorten some events so they don't overlap with event starting later
    /// </summary>
    private void ShortenEvents()
    {
        foreach (VEvent evnt in calendar.VCalendar.Events)
        {
            if (evnt.IsAllDayEvent())
                // this is a whole day event. It should not be shortened
                continue;

            DateTime startDate = evnt.StartDateTime.DateTimeValue;
            DateTime endDate = evnt.EndDateTime.DateTimeValue;


            // see if there are any events starting between those two dates
            List<VEvent> shorteningEvents = calendar.VCalendar.Events.Where(ev =>
                ev.StartDateTime.DateTimeValue > startDate &&
                ev.StartDateTime.DateTimeValue < endDate).ToList();

            if (shorteningEvents.Count > 0)
            {
                // event needs to be shortened
                // which of the following events starts first
                DateTime newEndDate = shorteningEvents.Min(ev => ev.StartDateTime.DateTimeValue).AddMinutes(-1);
                evnt.EndDateTime.DateTimeValue = newEndDate;
            }
        }
    }

    /// <summary>
    ///     Shorten events that are longer than X minutes to Y minutes
    /// </summary>
    private void ShortenEvents(int longerThanMinutes, int toMinutes)
    {
        foreach (VEvent evnt in calendar.VCalendar.Events)
            if (!evnt.IsAllDayEvent() && evnt.DurationBasedOnDates().TotalMinutes > longerThanMinutes)
                evnt.EndDateTime.DateTimeValue = evnt.StartDateTime.DateTimeValue.AddMinutes(toMinutes);
    }

    /// <summary>
    ///     Removes events that contain a given string in event's title (iCalendar Summary field)
    /// </summary>
    private void HideEventsContainingString(string stringPart)
    {
        List<string> blacklistedPhrases = stringPart.Split(';').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

        //calendar.VCalendar.Events.Remove is a hot line, requests take up to 6 seconds ;/
        // attempt of optimization:
        HashSet<VEvent> eventsToRemove = new HashSet<VEvent>(
            calendar.VCalendar.Events.Where(
                x => blacklistedPhrases.Any(
                    phrase => x.Summary.Value.ContainsIgnoreCase(phrase)
                )
            )
        );

        calendar.VCalendar.Events.RemoveAll(x => eventsToRemove.Contains(x));
    }

    /// <summary>
    ///     Removes all events that are shorter than given number of minutes. Events with duration time equal to a given
    ///     parameter will not be removed.
    /// </summary>
    private void HideEventsShorterThanMinutes(int minutes)
    {
        List<VEvent> eventsToRemove = calendar.VCalendar.Events
            .Where(evnt => evnt.DurationBasedOnDates().TotalMinutes < minutes).ToList();
        foreach (VEvent evnt in eventsToRemove)
            calendar.VCalendar.Events.Remove(evnt);
    }

    /// <summary>
    ///     Skips events that are connected to any of the given Todoist tasks (based on iCalendar Description field added by
    ///     Todoist when exporting items to calendar)
    /// </summary>
    private void SkipEventsFromProjects(IList<string> projectsToSkip)
    {
        List<VEvent> eventsToRemove = calendar.VCalendar.Events.Where(evnt => projectsToSkip.Contains(evnt.ProjectName()))
            .ToList();
        foreach (VEvent evnt in eventsToRemove)
            calendar.VCalendar.Events.Remove(evnt);
    }

    /// <summary>
    ///     Removes location information from events that contain "Microsoft Teams Meeting" in their location field.
    ///     The idea is to help declutter the "day" view (or N-day view) in the Fastmail Calendar, Outlook Calendar etc.
    /// </summary>
    private void RemoveTeamsLocations()
    {
        foreach (var evnt in calendar.VCalendar.Events.OfType<VEvent>())
        {
            // Check if the event's location contains "Microsoft Teams Meeting" (case-insensitive)
            if (!string.IsNullOrEmpty(evnt.Location.Value) &&
                evnt.Location.Value.IndexOf("Microsoft Teams Meeting", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Clear the location field
                evnt.Location.Value = null;
            }
        }
    }
}
