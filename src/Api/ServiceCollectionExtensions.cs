#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0053 // Use expression body for lambda expressions

namespace SearchService.Api;

using Authentication;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSwag;
using NSwag.Generation.Processors.Security;
using ZymLabs.NSwag.FluentValidation;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures the ForwardedHeadersOptions.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The services with options services added.</returns>
    public static IServiceCollection AddHostingOptions(
        this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto;
            // Only loopback proxies are allowed by default.
            // Clear that restriction because forwarders are enabled by explicit
            // configuration.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        return services;
    }

    /// <summary>
    ///     Adds custom routing settings which determines how URL's are generated.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The services with routing services added.</returns>
    public static IServiceCollection AddCustomRouting(this IServiceCollection services) =>
        services.AddRouting(options => options.LowercaseUrls = true);

    /// <summary>
    ///     Adds custom versioning settings and format.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services) => services
        .AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = new HeaderApiVersionReader("X-Version");
        })
        .AddVersionedApiExplorer(x =>
        {
            //x.SubstituteApiVersionInUrl = true;
            x.GroupNameFormat = "'v'VVV";
        }); // Version format: 'v'major[.minor][-status]

    /// <summary>
    ///     Adds custom authentication.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="environment"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services.AddKeycloakAuthentication(configuration, options =>
        {
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateIssuer = true;
        });

        services.AddAuthZ();

        if (environment.IsDevelopment()
            && configuration.GetValue<bool>("UseStubAuthentication"))
        {
            // Disable authentication and authorization.
            services.RemoveAll<IPolicyEvaluator>();
            services.TryAddSingleton<
                IPolicyEvaluator, DisableAuthenticationPolicyEvaluator>();
        }

        return services;
    }

    /// <summary>
    ///     Adds Swagger services and configures the Swagger services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration"></param>
    /// <returns>The services with Swagger services added.</returns>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FluentValidationSchemaProcessor>();

        var apiVersionProvider = services.BuildServiceProvider()
            .GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var apiVersionDescription in apiVersionProvider.ApiVersionDescriptions)
        {
            services.AddOpenApiDocument((options, serviceProvider) =>
            {
                var version = apiVersionDescription.ApiVersion.ToString();
                options.Version = version;
                options.DocumentName = version;
                options.PostProcess = document =>
                {
                    document.Info.Title = "api-service";
                    document.Info.Description = "";
                    document.Info.Version = version;
                };
                options.ApiGroupNames = new[] { apiVersionDescription.GroupName };

                var identityProviderUrl = serviceProvider
                    .GetRequiredService<KeycloakInstallationOptions>().KeycloakUrlRealm;
                var tokenUrl =
                    $"{identityProviderUrl}/{configuration.GetValue<string>("IdentityProvider:TokenEndpoint")}";
                var authorizationEndpoint =
                    $"{identityProviderUrl}/{configuration.GetValue<string>("IdentityProvider:AuthorizationEndpoint")}";

                options.AddSecurity("oauth2", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.OAuth2,
                        Description = "OAuth2 Client Authorization",
                        Flow = OpenApiOAuth2Flow.Implicit,
                        TokenUrl = tokenUrl,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                Scopes = new Dictionary<string, string>(),
                                AuthorizationUrl = authorizationEndpoint,
                            },
                        }
                    });
                // Manually provide generated JWT
                options.AddSecurity("JWT", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}."
                    });

                options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("oauth2"));
                options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));

                var fluentValidationSchemaProcessor = serviceProvider
                    .GetService<FluentValidationSchemaProcessor>();
                options.SchemaProcessors.Add(fluentValidationSchemaProcessor);
            });
        }

        return services;
    }

    /// <summary>
    ///     Adds cross-origin resource sharing (CORS) services and configures named CORS policies. See
    ///     https://docs.asp.net/en/latest/security/cors.html
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="policyName"></param>
    /// <param name="configuration"></param>
    /// <returns>The services with CORS services added.</returns>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, string policyName,
        IConfiguration configuration)
    {
        // Create named CORS policies here which you can consume using application.UseCors("PolicyName")
        // or a [EnableCors("PolicyName")] attribute on your controller or action.

        var origins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        return services.AddCors(builder =>
            builder.AddPolicy(policyName,
                x => x
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("content-disposition")));
    }

    /// <summary>
    ///     Adds health check to the application.
    /// </summary>
    /// <param name="services">The services. </param>
    /// <returns>The services with health check services added.</returns>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services) =>
        services
            .AddHealthChecks()
            .Services;
}
