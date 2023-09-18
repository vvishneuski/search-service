#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0053 // Use expression body for lambda expressions

namespace SearchService.Api;

using NSwag.AspNetCore;
using Serilog;
using Serilog.Events;

internal static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Uses custom serilog request logging. Adds additional properties to each log.
    ///     See https://github.com/serilog/serilog-aspnetcore.
    /// </summary>
    /// <param name="application">The application builder.</param>
    /// <returns>The application builder with the Serilog middleware configured.</returns>
    public static IApplicationBuilder UseCustomSerilogRequestLogging(this IApplicationBuilder application) =>
        application.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var request = httpContext.Request;
                var response = httpContext.Response;
                var endpoint = httpContext.GetEndpoint();

                diagnosticContext.Set("Host", request.Host);
                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("Scheme", request.Scheme);

                if (request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", request.QueryString.Value);
                }

                if (endpoint != null)
                {
                    diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                }

                diagnosticContext.Set("ContentType", response.ContentType);
            };
            options.GetLevel = GetLevel;

            static LogEventLevel GetLevel(HttpContext httpContext, double elapsedMilliseconds, Exception exception)
            {
                if (exception == null && httpContext.Response.StatusCode <= 499)
                {
                    if (IsHealthCheckEndpoint(httpContext))
                    {
                        return LogEventLevel.Verbose;
                    }

                    return LogEventLevel.Information;
                }

                return LogEventLevel.Error;

                static bool IsHealthCheckEndpoint(HttpContext httpContext)
                {
                    var endpoint = httpContext.GetEndpoint();
                    if (endpoint != null)
                    {
                        return endpoint.DisplayName == "Health checks";
                    }

                    return false;
                }
            }
        });

    /// <summary>
    ///     Register OpenAPI toolchain.
    /// </summary>
    /// <param name="application"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder application,
        IConfiguration configuration)
    {
        // https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware
        // https://github.com/RicoSuter/NSwag/blob/master/samples/WithMiddleware/Sample.AspNetCore21.Nginx/Startup.cs
        var swaggerDocumentPath = "api/swagger/{documentName}/swagger.json";

        application.UseOpenApi(options =>
        {
            options.Path = swaggerDocumentPath;
        });
        application.UseSwaggerUi3(options =>
        {
            var clientId = configuration.GetValue<string>("IdentityProvider:SwaggerClientId");
            options.Path = "/api/swagger";
            options.DocumentPath = swaggerDocumentPath;
            options.OAuth2Client = new OAuth2ClientSettings { AppName = $"{clientId} - app", ClientId = clientId, };
        });
        return application;
    }
}
