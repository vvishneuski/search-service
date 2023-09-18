namespace SearchService.Api;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    ///     Adds a Health Check endpoint to the <see cref="IEndpointRouteBuilder" /> with the specified template.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder" /> to add endpoint to.</param>
    /// <param name="pattern">The URL pattern of the endpoint.</param>
    /// <param name="servicesPattern"></param>
    /// <returns>A route for the endpoint.</returns>
    public static IEndpointConventionBuilder MapCustomHealthCheck(
        this IEndpointRouteBuilder endpoints, string pattern, string servicesPattern)
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        endpoints.MapHealthChecks(pattern, new HealthCheckOptions
        {
            ResponseWriter = WriteResponse,
            Predicate = check => !check.Tags.Contains("services")
                                 && !check.Tags.Contains("ready"),
            AllowCachingResponses = false,
        }).AllowAnonymous();

        return endpoints.MapHealthChecks(servicesPattern,
            new HealthCheckOptions
            {
                ResponseWriter = WriteResponse, Predicate = check => true, AllowCachingResponses = false,
            }).AllowAnonymous();

        static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var report = result.Entries;

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(report.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description)))))));

            return context.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
    }
}
