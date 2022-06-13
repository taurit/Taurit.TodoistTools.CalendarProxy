using EWSoftware.PDI.Parser;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;

namespace Taurit.TodoistTools.CalendarProxy.Controllers;

public class ProxyController : Controller
{
    /// <summary>
    ///     Parses parameters provided by user in QueryString, downloads calendar, filters events based on known rules
    /// </summary>
    /// <returns></returns>
    public async Task<ActionResult> Filter(CancellationToken ct)
    {
        using HttpClient? webClient = new HttpClient()
        {
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            DefaultRequestHeaders =
            {
                // Office365 api started to failing with unclear 400 ArgumentNullException if User-Agent header was not present in the request
                UserAgent = { new ProductInfoHeaderValue("HttpClient", "0.0.1") }
            }
        };
        try
        {
            // parse request parameters and validate requested action
            FilteringOptions? options = new FilteringOptions(Request.QueryString.Value);
            if (options.CalendarUrl == null)
                throw new ArgumentException("calendarUrl parameter was not provided by the user");

            if (!(options.CalendarUrl.Scheme.ToLowerInvariant() == "https" ||
                  options.CalendarUrl.Scheme.ToLowerInvariant() == "http"))
                return BadRequest("Specified protocol was not recognized. Url should begin with 'http' or 'https'.");

            // download the source iCalendar file content
            string icalContent = await webClient.GetStringAsync(options.CalendarUrl, ct);

            // parse iCalendar and filter according to user-defined options
            EventManager eventManager = new EventManager(icalContent);
            eventManager.Filter(options);

            // return filtered calendar as a response in iCalendar format
            string? icalResponse = eventManager.GetIcal();
            return Content(icalResponse);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (UriFormatException)
        {
            return BadRequest("Specified URL was not valid.");
        }
        catch (WebException)
        {
            return BadRequest("Specified resource could not have been accessed.");
        }
        catch (PDIParserException)
        {
            return BadRequest("Requested file is not a valid iCalendar file.");
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, "Application exception");
        }
    }
}
