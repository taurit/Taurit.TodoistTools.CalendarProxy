using System.Collections.Specialized;
using System.Web;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers
{
    /// <summary>
    ///     Configuration defining operations that should be performed on an input calendar
    /// </summary>
    public class FilteringOptions
    {
        private bool hideAllDayEvents;
        private bool hidePrivateEvents;
        private bool predictEventDuration;
        private bool removeEventDurationFromTitle;
        private bool shortenEvents;


        /// <summary>
        ///     Default constructor, initialize object with default settings, which should not affect input calendar at all
        /// </summary>
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
        }

        /// <summary>
        ///     Initialize options with settings obtained from URL parameters.
        ///     For compatibility with most calendar programs, parameters must be encoded in URL's GET parameters.
        ///     Also, although RFC documents are vague about maximum URL length, currently the practical limit that works in all
        ///     browsers
        ///     is about 2000 characters. Therefore QueryString keys were deliberately chosen to be as short as possible, even
        ///     though it affects
        ///     readability of such URL.
        /// </summary>
        /// <param name="queryString">
        ///     Valid HTTP QueryString. Currently the following parameters are recognized:
        ///     h: HideAllDayEvents,
        ///     hp: HidePrivateEvents,
        ///     s: ShortenEvents,
        ///     p: PredictEventDuration
        ///     r: RemoveEventDurationFromTitle
        ///     st: SkipEventsContainingThisString
        ///     min: HideEventsShorterThanMinutes
        ///     pr: HideEventsFromThoseProjects
        ///     lt: ShortenEventsLongerThanThisMinutes
        ///     mt: ShortenEventsLongerThanThisMinutes
        /// </param>
        public FilteringOptions(NameValueCollection qsParams)
        {
            ParseParameters(qsParams);
        }

        /// <summary>
        ///     Constructor overload using raw QueryString instead of NameValueCollection of parameters
        /// </summary>
        /// <param name="qsParamsRaw"></param>
        public FilteringOptions(string qsParamsRaw)
        {
            NameValueCollection qsParams = HttpUtility.ParseQueryString(qsParamsRaw);
            ParseParameters(qsParams);
        }

        /// <summary>
        ///     Original calendar URL
        /// </summary>
        public Uri CalendarUrl { get; set; }

        /// <summary>
        ///     Hide all-day events in the output
        /// </summary>
        public bool HideAllDayEvents
        {
            get => hideAllDayEvents;
            private set => hideAllDayEvents = value;
        }

        /// <summary>
        ///     Hide private events in the output
        /// </summary>
        public bool HidePrivateEvents
        {
            get => hidePrivateEvents;
            private set => hidePrivateEvents = value;
        }

        /// <summary>
        ///     Shorten events, so they never overlap next event in the calendar
        /// </summary>
        public bool ShortenEvents
        {
            get => shortenEvents;
            private set => shortenEvents = value;
        }

        /// <summary>
        ///     Predict the duration of the event based on event title, which might contain this information
        /// </summary>
        public bool PredictEventDuration
        {
            get => predictEventDuration;
            private set => predictEventDuration = value;
        }

        /// <summary>
        ///     If regex containing recognized duration of event is found in the title, remove this fragment
        /// </summary>
        public bool RemoveEventDurationFromTitle
        {
            get => removeEventDurationFromTitle;
            private set => removeEventDurationFromTitle = value;
        }

        /// <summary>
        ///     If the string is not null or empty, events containing this string will be hidden
        /// </summary>
        public string HideEventsContainingThisString { get; private set; }

        /// <summary>
        ///     If this value is greater than 0, events shorten than this value will be hidden
        /// </summary>
        public int HideEventsShorterThanMinutes { get; private set; }

        public int ShortenEventsLongerThanThisMinutes { get; private set; }
        public int ShortenEventsLongerThanToThisMinutes { get; private set; }

        /// <summary>
        ///     Comma-delimited list of Todoist projects which should not be displayed in this calendar
        /// </summary>
        public string HideEventsFromThoseProjects { get; private set; }

        public IList<string> ProjectsToSkip
        {
            get
            {
                List<string> projectNames = new List<string>();
                if (!string.IsNullOrWhiteSpace(HideEventsFromThoseProjects))
                    projectNames = HideEventsFromThoseProjects.Split(',').Select(x => x.Trim()).ToList();
                return projectNames;
            }
        }

        /// <summary>
        ///     Parse QueryString parameters collection as a set of filtering options
        /// </summary>
        /// <param name="qsParams"></param>
        private void ParseParameters(NameValueCollection qsParams)
        {
            string calendarUrl = qsParams["calendarUrl"] == null ? null : qsParams["calendarUrl"];
            if (calendarUrl != null)
                CalendarUrl = new Uri(calendarUrl);

            TrySetBool(qsParams, "h", ref hideAllDayEvents);
            TrySetBool(qsParams, "hp", ref hidePrivateEvents);
            TrySetBool(qsParams, "s", ref shortenEvents);
            TrySetBool(qsParams, "p", ref predictEventDuration);
            TrySetBool(qsParams, "r", ref removeEventDurationFromTitle);

            if (!string.IsNullOrWhiteSpace(qsParams["st"]))
                HideEventsContainingThisString = qsParams["st"].Trim();

            if (!string.IsNullOrWhiteSpace(qsParams["min"]))
            {
                int parsedParam = 0;
                if (int.TryParse(qsParams["min"], out parsedParam) && parsedParam > 0)
                    HideEventsShorterThanMinutes = parsedParam;
            }

            // those two params are tied
            if (!string.IsNullOrWhiteSpace(qsParams["lt"]) && !string.IsNullOrWhiteSpace(qsParams["mt"]))
            {
                int parsedParam = 0;
                if (int.TryParse(qsParams["lt"], out parsedParam) && parsedParam > 0)
                    ShortenEventsLongerThanThisMinutes = parsedParam;
                if (int.TryParse(qsParams["mt"], out parsedParam) && parsedParam > 0)
                    ShortenEventsLongerThanToThisMinutes = parsedParam;

                // make sure that both parameters were provided correctly
                if (ShortenEventsLongerThanThisMinutes == 0 || ShortenEventsLongerThanToThisMinutes == 0
                                                            || ShortenEventsLongerThanToThisMinutes >
                                                            ShortenEventsLongerThanThisMinutes)
                {
                    ShortenEventsLongerThanThisMinutes = 0;
                    ShortenEventsLongerThanToThisMinutes = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(qsParams["pr"]))
                HideEventsFromThoseProjects = qsParams["pr"].Trim();
        }

        /// <summary>
        ///     Parse user-provided string value as bool
        /// </summary>
        /// <param name="qsParams"></param>
        /// <param name="param"></param>
        /// <param name="prop"></param>
        private void TrySetBool(NameValueCollection qsParams, string param, ref bool prop)
        {
            bool localBool = false;
            if (qsParams[param] != null)
                switch (qsParams[param])
                {
                    case "1":
                        prop = true;
                        break;
                    case "0":
                        prop = false;
                        break;
                    default:
                        if (bool.TryParse(qsParams[param], out localBool))
                            prop = localBool;
                        break;
                }
        }
    }
}