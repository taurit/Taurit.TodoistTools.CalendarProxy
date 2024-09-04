using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using EWSoftware.PDI.Parser;
using System.Net;
using System.Net.Http.Headers;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Taurit.TodoistTools.CalendarProxy.Serverless;

/// <summary>
/// I moved from Azure App Service to AWS Lambda to solve the issue of high cold start times of ASP.NET Core apps in Azure's free plans.
/// High cold start times lead to timeouts in calendar apps/services like Fastmail and calendar quietly stopped to sync.
/// </summary>
public class Functions
{
    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Get, "/filter")]
    public async Task<IHttpResult> Filter(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        using HttpClient webClient = new HttpClient()
        {
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            DefaultRequestHeaders =
            {
                // Added because Office365 API started to failing with unclear 400 ArgumentNullException
                // if User-Agent header was not present in the request
                UserAgent = { new ProductInfoHeaderValue("HttpClient", "0.0.1") }
            }
        };
        try
        {
            // parse request parameters and validate requested action
            FilteringOptions options = new FilteringOptions(request.RawQueryString);

            if (options.CalendarUrl == null)
                throw new ArgumentException("calendarUrl parameter was not provided by the user");

            if (!(options.CalendarUrl.Scheme.ToLowerInvariant() == "https" ||
                  options.CalendarUrl.Scheme.ToLowerInvariant() == "http"))
                return HttpResults.BadRequest("Specified protocol was not recognized. Url should begin with 'http' or 'https'.");

            // download the source iCalendar file content
            string icalContent = await webClient.GetStringAsync(options.CalendarUrl);

            // parse iCalendar and filter according to user-defined options
            EventManager eventManager = new EventManager(icalContent);
            eventManager.Filter(options);

            // return filtered calendar as a response in iCalendar format
            string icalResponse = eventManager.GetIcal();
            return HttpResults.Ok(icalResponse);
        }
        catch (ArgumentException ae)
        {
            return HttpResults.BadRequest(ae.Message);
        }
        catch (UriFormatException)
        {
            return HttpResults.BadRequest("Specified URL was not valid.");
        }
        catch (WebException)
        {
            return HttpResults.BadRequest("Specified resource could not have been accessed.");
        }
        catch (PDIParserException)
        {
            return HttpResults.BadRequest("Requested file is not a valid iCalendar file.");
        }
        catch (Exception)
        {
            return HttpResults.InternalServerError("Application exception");
        }
    }

}