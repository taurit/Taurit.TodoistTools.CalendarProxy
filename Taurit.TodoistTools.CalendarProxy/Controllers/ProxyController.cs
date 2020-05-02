using System;
using System.Net;
using System.Text;
using EWSoftware.PDI.Parser;
using Microsoft.AspNetCore.Mvc;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;

namespace Taurit.TodoistTools.CalendarProxy.Controllers
{
    public class ProxyController : Controller
    {
        /// <summary>
        ///     Parses parameters provided by user in QueryString, downloads calendar, filters events based on known rules
        /// </summary>
        /// <returns></returns>
        public ActionResult Filter()
        {
            using var webClient = new WebClient();
            try
            {
                // parse request parameters and validate requested action
                var options = new FilteringOptions(this.Request.QueryString.Value);
                if (options.CalendarUrl == null)
                    throw new ArgumentException("calendarUrl parameter was not provided by the user");

                if (!(options.CalendarUrl.Scheme.ToLowerInvariant() == "https" ||
                      options.CalendarUrl.Scheme.ToLowerInvariant() == "http"))
                    return BadRequest("Specified protocol was not recognized. Url should begin with 'http' or 'https'.");

                // download the source iCalendar file content
                webClient.Encoding = Encoding.UTF8;
                var icalContent = webClient.DownloadString(options.CalendarUrl);

                // parse iCalendar and filter according to user-defined options
                var eventManager = new EventManager(icalContent);
                eventManager.Filter(options);

                // return filtered calendar as a response in iCalendar format
                var icalResponse = eventManager.GetIcal();
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
                return StatusCode((int) HttpStatusCode.InternalServerError, "Application exception");
            }
        }
    }
}