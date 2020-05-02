using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers
{
    /// <summary>
    /// Configuration defining operations that should be performed on an input calendar
    /// </summary>
    public class FilteringOptions
    {
        /// <summary>
        /// Original calendar URL
        /// </summary>
        public Uri CalendarUrl { get; set; }

        /// <summary>
        /// Hide all-day events in the output
        /// </summary>
        public bool HideAllDayEvents { get { return hideAllDayEvents; } private set { hideAllDayEvents = value; } }
        private bool hideAllDayEvents;

        /// <summary>
        /// Shorten events, so they never overlap next event in the calendar
        /// </summary>
        public bool ShortenEvents { get { return shortenEvents; } private set { shortenEvents = value; } }
        private bool shortenEvents;

        /// <summary>
        /// Predict the duration of the event based on event title, which might contain this information
        /// </summary>
        public bool PredictEventDuration { get { return predictEventDuration; } private set { predictEventDuration = value; } }
        private bool predictEventDuration;

        /// <summary>
        /// If regex containing recognized duration of event is found in the title, remove this fragment
        /// </summary>
        public bool RemoveEventDurationFromTitle { get { return removeEventDurationFromTitle; } private set { removeEventDurationFromTitle = value; } }
        private bool removeEventDurationFromTitle;

        /// <summary>
        /// If the string is not null or empty, events containing this string will be hidden
        /// </summary>
        public string HideEventsContainingThisString { get; private set; }

        /// <summary>
        /// If this value is greater than 0, events shorten than this value will be hidden
        /// </summary>
        public int HideEventsShorterThanMinutes { get; private set; }

        public int ShortenEventsLongerThanThisMinutes { get; private set; }
        public int ShortenEventsLongerThanToThisMinutes { get; private set; }

        /// <summary>
        /// Comma-delimited list of Todoist projects which should not be displayed in this calendar
        /// </summary>
        public string HideEventsFromThoseProjects { get; private set; }
        public IList<string> ProjectsToSkip
        {
            get
            {
                List<string> projectNames = new List<string>();
                if (!String.IsNullOrWhiteSpace(this.HideEventsFromThoseProjects))
                {
                    projectNames = this.HideEventsFromThoseProjects.Split(',').Select(x => x.Trim()).ToList();
                }
                return projectNames;
            }
        }


        /// <summary>
        /// Default constructor, initialize object with default settings, which should not affect input calendar at all
        /// </summary>
        public FilteringOptions()
        {
            this.HideAllDayEvents = false;
            this.ShortenEvents = false;
            this.PredictEventDuration = false;
            this.RemoveEventDurationFromTitle = false;
            this.HideEventsContainingThisString = null;
            this.HideEventsShorterThanMinutes = 0;
            this.HideEventsFromThoseProjects = null;
            this.ShortenEventsLongerThanThisMinutes = 0;
            this.ShortenEventsLongerThanToThisMinutes = 0;
        }

        /// <summary>
        /// Initialize options with settings obtained from URL parameters.
        /// For compatibility with most calendar programs, parameters must be encoded in URL's GET parameters.
        /// Also, although RFC documents are vague about maximum URL length, currently the practical limit that works in all browsers
        /// is about 2000 characters. Therefore QueryString keys were deliberately chosen to be as short as possible, even though it affects
        /// readability of such URL.
        /// </summary>
        /// <param name="queryString">Valid HTTP QueryString. Currently the following parameters are recognized:
        /// h: HideAllDayEvents,
        /// s: ShortenEvents,
        /// p: PredictEventDuration
        /// r: RemoveEventDurationFromTitle
        /// st: SkipEventsContainingThisString
        /// min: HideEventsShorterThanMinutes
        /// pr: HideEventsFromThoseProjects
        /// lt: ShortenEventsLongerThanThisMinutes 
        /// mt: ShortenEventsLongerThanThisMinutes 
        /// </param>
        public FilteringOptions(NameValueCollection qsParams)
        {
            this.ParseParameters(qsParams);
        }

        /// <summary>
        /// Constructor overload using raw QueryString instead of NameValueCollection of parameters
        /// </summary>
        /// <param name="qsParamsRaw"></param>
        public FilteringOptions(string qsParamsRaw)
        {
            NameValueCollection qsParams = HttpUtility.ParseQueryString(qsParamsRaw);
            this.ParseParameters(qsParams);
        }

        /// <summary>
        /// Parse QueryString parameters collection as a set of filtering options
        /// </summary>
        /// <param name="qsParams"></param>
        private void ParseParameters(NameValueCollection qsParams)
        {
            var calendarUrl = qsParams["calendarUrl"] == null ? null : qsParams["calendarUrl"].ToString();
            if (calendarUrl != null)
                this.CalendarUrl = new Uri(calendarUrl);

            this.TrySetBool(qsParams, "h", ref hideAllDayEvents);
            this.TrySetBool(qsParams, "s", ref shortenEvents);
            this.TrySetBool(qsParams, "p", ref predictEventDuration);
            this.TrySetBool(qsParams, "r", ref removeEventDurationFromTitle);

            if (!String.IsNullOrWhiteSpace(qsParams["st"]))
                this.HideEventsContainingThisString = qsParams["st"].Trim();

            if (!String.IsNullOrWhiteSpace(qsParams["min"]))
            {
                int parsedParam = 0;
                if (Int32.TryParse(qsParams["min"], out parsedParam) && parsedParam > 0)
                    this.HideEventsShorterThanMinutes = parsedParam;
            }

            // those two params are tied
            if (!String.IsNullOrWhiteSpace(qsParams["lt"]) && !String.IsNullOrWhiteSpace(qsParams["mt"]))
            {
                int parsedParam = 0;
                if (Int32.TryParse(qsParams["lt"], out parsedParam) && parsedParam > 0)
                    this.ShortenEventsLongerThanThisMinutes = parsedParam;
                if (Int32.TryParse(qsParams["mt"], out parsedParam) && parsedParam > 0)
                    this.ShortenEventsLongerThanToThisMinutes = parsedParam;

                // make sure that both parameters were provided correctly
                if (this.ShortenEventsLongerThanThisMinutes == 0 || this.ShortenEventsLongerThanToThisMinutes == 0
                    || this.ShortenEventsLongerThanToThisMinutes > this.ShortenEventsLongerThanThisMinutes)
                {
                    this.ShortenEventsLongerThanThisMinutes = 0;
                    this.ShortenEventsLongerThanToThisMinutes = 0;
                }
            }

            if (!String.IsNullOrWhiteSpace(qsParams["pr"]))
                this.HideEventsFromThoseProjects = qsParams["pr"].Trim();

        }

        /// <summary>
        /// Parse user-provided string value as bool
        /// </summary>
        /// <param name="qsParams"></param>
        /// <param name="param"></param>
        /// <param name="prop"></param>
        private void TrySetBool(NameValueCollection qsParams, string param, ref bool prop)
        {
            bool localBool = false;
            if (qsParams[param] != null)
            {
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
}