using System.Collections.Specialized;
using System.Web;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers
{
    // Configuration defining operations that should be performed on an input calendar
    public class FilteringOptions
    {
        // Default constructor; initializes object with default settings that do not affect the input calendar.
        public FilteringOptions()
        {
            HideAllDayEvents = false;
            HidePrivateEvents = false;
            ShortenEvents = false;
            PredictEventDuration = false;
            RemoveEventDurationFromTitle = false;
            HideEventsContainingThisString = null;
            HideEventsShorterThanMinutes = 0;
            HideEventsFromThoseProjects = null;
            ShortenEventsLongerThanThisMinutes = 0;
            ShortenEventsLongerThanToThisMinutes = 0;
            RemoveTeamsLocations = false;
        }

        // Initializes options with settings obtained from URL parameters.
        // Valid HTTP QueryString. Recognized parameters:
        // h: HideAllDayEvents, hp: HidePrivateEvents, s: ShortenEvents,
        // p: PredictEventDuration, r: RemoveEventDurationFromTitle,
        // st: SkipEventsContainingThisString, min: HideEventsShorterThanMinutes,
        // pr: HideEventsFromThoseProjects, lt: ShortenEventsLongerThanThisMinutes,
        // mt: ShortenEventsLongerThanThisMinutes, rtl: RemoveTeamsLocations
        public FilteringOptions(NameValueCollection qsParams) => ParseParameters(qsParams);

        // Constructor overload using raw QueryString parameters.
        public FilteringOptions(string qsParamsRaw)
            : this(HttpUtility.ParseQueryString(qsParamsRaw))
        {
        }

        // Original calendar URL.
        public Uri CalendarUrl { get; private set; }

        // Hide all-day events in the output.
        public bool HideAllDayEvents { get; private set; }

        // Hide private events in the output.
        public bool HidePrivateEvents { get; private set; }

        // Shorten events so they never overlap with the next event in the calendar.
        public bool ShortenEvents { get; private set; }

        // Predict the duration of an event based on its title.
        public bool PredictEventDuration { get; private set; }

        // Remove fragments from the title that contain recognized duration.
        public bool RemoveEventDurationFromTitle { get; private set; }

        // If not null or empty, events containing this string will be hidden.
        public string HideEventsContainingThisString { get; private set; }

        // If greater than 0, events shorter than this value will be hidden.
        public int HideEventsShorterThanMinutes { get; private set; }

        public int ShortenEventsLongerThanThisMinutes { get; private set; }
        public int ShortenEventsLongerThanToThisMinutes { get; private set; }

        // Comma-delimited list of Todoist projects that should not be displayed.
        public string HideEventsFromThoseProjects { get; private set; }

        // Remove Microsoft Teams locations from events without hiding them.
        public bool RemoveTeamsLocations { get; private set; }

        // Returns a list of project names parsed from HideEventsFromThoseProjects.
        public IList<string> ProjectsToSkip =>
            string.IsNullOrWhiteSpace(HideEventsFromThoseProjects)
                ? new List<string>()
                : HideEventsFromThoseProjects.Split(',').Select(x => x.Trim()).ToList();

        // Parse QueryString parameters as filtering options.
        private void ParseParameters(NameValueCollection qsParams)
        {
            var calendarUrl = qsParams["calendarUrl"];
            if (!string.IsNullOrWhiteSpace(calendarUrl))
            {
                CalendarUrl = new Uri(calendarUrl);
            }

            HideAllDayEvents = ParseBool(qsParams["h"]);
            HidePrivateEvents = ParseBool(qsParams["hp"]);
            ShortenEvents = ParseBool(qsParams["s"]);
            PredictEventDuration = ParseBool(qsParams["p"]);
            RemoveEventDurationFromTitle = ParseBool(qsParams["r"]);
            RemoveTeamsLocations = ParseBool(qsParams["rtl"]);

            if (!string.IsNullOrWhiteSpace(qsParams["st"]))
            {
                HideEventsContainingThisString = qsParams["st"].Trim();
            }

            if (!string.IsNullOrWhiteSpace(qsParams["min"]) &&
                int.TryParse(qsParams["min"], out int parsedMin) && parsedMin > 0)
            {
                HideEventsShorterThanMinutes = parsedMin;
            }

            // Parse tied parameters for shortening events.
            if (!string.IsNullOrWhiteSpace(qsParams["lt"]) && !string.IsNullOrWhiteSpace(qsParams["mt"]))
            {
                if (int.TryParse(qsParams["lt"], out int lt) && lt > 0)
                {
                    ShortenEventsLongerThanThisMinutes = lt;
                }
                if (int.TryParse(qsParams["mt"], out int mt) && mt > 0)
                {
                    ShortenEventsLongerThanToThisMinutes = mt;
                }

                // Ensure parameter validity.
                if (ShortenEventsLongerThanThisMinutes == 0 ||
                    ShortenEventsLongerThanToThisMinutes == 0 ||
                    ShortenEventsLongerThanToThisMinutes > ShortenEventsLongerThanThisMinutes)
                {
                    ShortenEventsLongerThanThisMinutes = 0;
                    ShortenEventsLongerThanToThisMinutes = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(qsParams["pr"]))
            {
                HideEventsFromThoseProjects = qsParams["pr"].Trim();
            }
        }

        // Helper method to parse a string value to a bool.
        private bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value switch
            {
                "1" => true,
                "0" => false,
                _ => bool.TryParse(value, out var result) && result
            };
        }
    }
}