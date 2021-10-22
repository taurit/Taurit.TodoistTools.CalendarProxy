using System.Globalization;

namespace Taurit.TodoistTools.CalendarProxy.Library.Helpers;

public static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string paragraph, string word)
    {
        return CultureInfo.CurrentCulture.CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
    }
}
