using EWSoftware.PDI.Objects;
using System.Text.RegularExpressions;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers;

public static class VEventExtensionMethods
{
    private static readonly Regex regexProjectName = new Regex(@"([A-Za-z]*): (.*?)\\n", RegexOptions.Multiline);

    public static TimeSpan DurationBasedOnDates(this VEvent evnt)
    {
        return evnt.EndDateTime.DateTimeValue.Subtract(evnt.StartDateTime.DateTimeValue);
    }

    /// <summary>
    ///     For Todoist task, returns name of the user's project assigned to the task based on description (which is
    ///     localized).
    /// </summary>
    /// <param name="evnt"></param>
    /// <returns>user's project assigned to the task or null if not available</returns>
    public static string? ProjectName(this VEvent evnt)
    {
        if (evnt.Description.Value != null)
        {
            Match m = regexProjectName.Match(evnt.Description.Value);
            if (m.Success) return m.Groups[2].Value;
        }

        return null;
    }

    public static bool IsAllDayEvent(this VEvent evnt)
    {
        DateTime startDate = evnt.StartDateTime.DateTimeValue;
        DateTime endDate = evnt.EndDateTime.DateTimeValue;

        return
            (startDate == endDate ||
             startDate.AddDays(1) == endDate
            ) && // event takes 0 or 24 hours - both options are used for full-day events
            startDate.Hour == 0 && startDate.Minute == 0;
    }

    public static bool IsPrivate(this VEvent evnt)
    {
        return (evnt.Classification.Value ?? "").ToLowerInvariant() == "private" ||
               (evnt.Summary.Value ?? "").ToLowerInvariant() == "private appointment";
    }
}
