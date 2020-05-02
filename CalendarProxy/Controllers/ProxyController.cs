using CalendarProxy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EWSoftware.PDI.Parser;

namespace CalendarProxy.Controllers
{
    public class ProxyController : Controller
    {
        /// <summary>
        /// Parses parameters provided by user in QueryString, downloads calendar, filters events based on known rules
        /// </summary>
        /// <returns></returns>
        public ActionResult Filter()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    // parse request parameters and validate requested action
                    FilteringOptions options = new FilteringOptions(Request.QueryString);
                    if (options.CalendarUrl == null)
                        throw new ArgumentException("calendarUrl parameter was not provided by the user");

                    if (!((options.CalendarUrl.Scheme.ToLowerInvariant() == "https") || (options.CalendarUrl.Scheme.ToLowerInvariant() == "http")))
                        throw new HttpException(400, "Specified protocol was not recognized. Url should begin with 'http' or 'https'.");

                    // download the source iCalendar file content
                    webClient.Encoding = Encoding.UTF8;
                    string icalContent = webClient.DownloadString(options.CalendarUrl);

                    // parse iCalendar and filter according to user-defined options
                    EventManager eventManager = new EventManager(icalContent);
                    eventManager.Filter(options);

                    // return filtered calendar as a response in iCalendar format
                    string icalResponse = eventManager.GetIcal();
                    return Content(icalResponse);
                }
                catch (ArgumentException ae)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ae.Message);
                }
                catch (UriFormatException)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Specified URL was not valid.");
                }
                catch (WebException)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Specified resource could not have been accessed.");
                }
                catch (PDIParserException)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Requested file is not a valid iCalendar file.");
                }
                catch (Exception)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Application exception");
                }
            }
        }
    }
}