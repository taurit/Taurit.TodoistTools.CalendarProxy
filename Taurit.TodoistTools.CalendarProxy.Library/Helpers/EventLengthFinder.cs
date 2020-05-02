using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers
{
    /// <summary>
    /// Looks for fragments indicating event length in the string
    /// </summary>
    public class EventLengthFinder
    {
        /// <summary>
        /// Regex to find patterns indicating event's length, for example: (1h 30 min)
        /// This regular expression is covered by unit tests in EventLengthFinderTests class
        /// </summary>
        public static Regex regexFindTime = new Regex(@"(\[|\()?([\d,\.]+)(\s*?)(h|hour|minute|minut|min|m)([s\s]|\z|\)|\])((\s*?)([\d,\.]+)(\s*?)(minutes|minute|minut|min|m)(\)|\])?)?",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

        /// <summary>
        /// Property indicating whether pattern looking like event's duration was found in a string
        /// </summary>
        public bool PatternFound { get; private set; }

        /// <summary>
        /// The unmodified string that was passed by the user
        /// </summary>
        private string OriginalString;

        /// <summary>
        /// The string that was passed by the user without the recognized regex part.
        /// If no pattern was faound, it has a null value.
        /// </summary>
        private string StringWithoutPattern = null;

        /// <summary>
        /// Examines task summery for fragments indicating event's length. If such fragments are found, 
        /// sets the property <see cref="PatternFound "/> to <value>true</value> and allows to get
        /// them as a normalized TimeSpan object.
        /// </summary>
        /// <param name="taskSummary">string that may contain some form of event duration</param>
        public EventLengthFinder(string taskSummary)
        {
            Match match = regexFindTime.Match(taskSummary);
            this.PatternFound = match.Success;
            this.OriginalString = taskSummary;

            if (match.Success)
            {
                decimal quantity = 0m;
                if (Decimal.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out quantity))
                {
                    string unit = match.Groups[4].Value;

                    if (unit.StartsWith("h", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // assume that unit is hours
                        this.totalMinutes = (int)(quantity * 60m);

                        // there might be another part of the string specifying minutes in this case
                        decimal quantityMinutesPart = 0m;
                        if (Decimal.TryParse(match.Groups[8].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out quantityMinutesPart))
                        {
                            this.totalMinutes += (int)quantityMinutesPart;
                        }

                    }
                    else
                    {
                        // assume that unit is minutes
                        this.totalMinutes = (int)quantity;
                    }

                    this.StringWithoutPattern = this.OriginalString.Replace(match.Groups[0].Value, "").Replace("()", "").Replace("[]", "").Replace("  ", " ").Trim();
                }
            }
        }

        /// <summary>
        /// If time pattern was faound in provided string, this value is normalized time that it represents (in minutes). Otherwise 0.
        /// </summary>
        private int totalMinutes = 0;

        /// <summary>
        /// Duration found in string (in minutes). If this property is obtained and there was no match, InvalidOperationException is thrown.
        /// </summary>
        public int TotalMinutes
        {
            get
            {
                if (this.PatternFound == false)
                    throw new InvalidOperationException("Time duration pattern was not found in a given string");

                return totalMinutes;
            }
        }

        /// <summary>
        /// If pattern was foundm returns a string with the found pattern cut. Otherwise returns original string.
        /// </summary>
        public string TaskSummaryWithoutPattern
        {
            get
            {
                return StringWithoutPattern;
            }
        }
    }
}