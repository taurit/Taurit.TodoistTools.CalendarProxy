using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Taurit.TodoistTools.CalendarProxy.Serverless;

/// <summary>
/// A "redundant" serverless project that can be deployed to AWS to solve the issue of high cold start times
/// of ASP.NET Core apps in Azure's free plans.
/// High cold start times lead to timeouts in Fastmail calendar and other tools and calendar quietly stops to sync.
/// </summary>
public class Functions
{
    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Get, "/filter/{calendarUrl}/{options}")]
    public IHttpResult Filter(string calendarUrl, string options, ILambdaContext context)
    {


        return HttpResults.Ok($"Calendar url is {calendarUrl}, option string is {options}");
    }

}