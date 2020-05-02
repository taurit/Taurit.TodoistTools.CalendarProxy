using System.Collections.Generic;
using System.Linq;
using EWSoftware.PDI.Objects;
using EWSoftware.PDI.Parser;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers
{
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
        /// <param name="icalContent"></param>
        public EventManager(string icalContent)
        {
            calendar = new VCalendarParser();
            calendar.ParseString(icalContent);
        }

        /// <summary>
        ///     Alternative constructor that initialises object based on given calendar object, without parsing anything
        /// </summary>
        /// <param name="calendar"></param>
        public EventManager(VCalendarParser calendar)
        {
            this.calendar = calendar;
        }

        /// <summary>
        ///     Filter events in loaded VCalendar based on provided filtering options
        /// </summary>
        /// <param name="filter"></param>
        public void Filter(FilteringOptions filter)
        {
            if (filter.PredictEventDuration)
                PredictEventDuration(filter.RemoveEventDurationFromTitle);

            if (filter.HideAllDayEvents)
                HideAllDayEvents(); // should be called before shortenig events

            var projectsToSkip = filter.ProjectsToSkip;
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
        ///     Returns calendar in iCalendar format, that might be returned to iCal-compatible orgranizer programs
        /// </summary>
        /// <returns></returns>
        public string GetIcal()
        {
            return calendar.VCalendar.ToString();
        }

        /// <summary>
        ///     Returns all events in current calendar as a generic list
        /// </summary>
        /// <returns></returns>
        public IList<VEvent> GetEventList()
        {
            return calendar.VCalendar.Events.OfType<VEvent>().ToList();
        }

        /// <summary>
        ///     Removes from calendar all events that start at midnight and last 24 hours
        /// </summary>
        private void HideAllDayEvents()
        {
            var eventsToRemove = new List<VEvent>();
            foreach (var evnt in calendar.VCalendar.Events.Where(evnt => evnt.IsAllDayEvent()))
                eventsToRemove.Add(evnt);

            foreach (var evnt in eventsToRemove)
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
            foreach (var evnt in calendar.VCalendar.Events)
            {
                if (evnt.IsAllDayEvent()) continue;
                var elf = new EventLengthFinder(evnt.Summary.Value);
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
            foreach (var evnt in calendar.VCalendar.Events)
            {
                if (evnt.IsAllDayEvent())
                    // this is a whole day event. It should not be shortened
                    continue;

                var startDate = evnt.StartDateTime.DateTimeValue;
                var endDate = evnt.EndDateTime.DateTimeValue;


                // see if there are any events starting between those two dates
                var shorteningEvents = calendar.VCalendar.Events.Where(ev =>
                    ev.StartDateTime.DateTimeValue > startDate &&
                    ev.StartDateTime.DateTimeValue < endDate).ToList();

                if (shorteningEvents.Count > 0)
                {
                    // event needs to be shortened
                    // which of the following events starts first
                    var newEndDate = shorteningEvents.Min(ev => ev.StartDateTime.DateTimeValue).AddMinutes(-1);
                    evnt.EndDateTime.DateTimeValue = newEndDate;
                }
            }
        }

        /// <summary>
        ///     Shorten events that are longer than X minutes to Y minutes
        /// </summary>
        /// <param name="longerThanMinutes"></param>
        /// <param name="toMinutes"></param>
        private void ShortenEvents(int longerThanMinutes, int toMinutes)
        {
            foreach (var evnt in calendar.VCalendar.Events)
                if (!evnt.IsAllDayEvent() && evnt.DurationBasedOnDates().TotalMinutes > longerThanMinutes)
                    evnt.EndDateTime.DateTimeValue = evnt.StartDateTime.DateTimeValue.AddMinutes(toMinutes);
        }

        /// <summary>
        ///     Removes events that contain a given string in event's title (iCalendar Summary field)
        /// </summary>
        /// <param name="stringPart"></param>
        private void HideEventsContainingString(string stringPart)
        {
            var eventsToRemove = calendar.VCalendar.Events.Where(evnt => evnt.Summary.Value.Contains(stringPart))
                .ToList();
            foreach (var evnt in eventsToRemove)
                calendar.VCalendar.Events.Remove(evnt);
        }

        /// <summary>
        ///     Removes all events that are shorter than given number of minutes. Events with duration time equal to a given
        ///     parameter will not be removed.
        /// </summary>
        /// <param name="minutes"></param>
        private void HideEventsShorterThanMinutes(int minutes)
        {
            var eventsToRemove = calendar.VCalendar.Events
                .Where(evnt => evnt.DurationBasedOnDates().TotalMinutes < minutes).ToList();
            foreach (var evnt in eventsToRemove)
                calendar.VCalendar.Events.Remove(evnt);
        }

        /// <summary>
        ///     Skips events that are connected to any of the given Todoist tasks (based on iCalendar Description field added by
        ///     Todoist when exporting items to calendar)
        /// </summary>
        /// <param name="projectsToSkip"></param>
        private void SkipEventsFromProjects(IList<string> projectsToSkip)
        {
            var eventsToRemove = calendar.VCalendar.Events.Where(evnt => projectsToSkip.Contains(evnt.ProjectName()))
                .ToList();
            foreach (var evnt in eventsToRemove)
                calendar.VCalendar.Events.Remove(evnt);
        }
    }
}