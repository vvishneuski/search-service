#pragma warning disable IDE0058 // Expression value is never used

namespace SearchService.Api;

using System.Reflection;
using Application;
using Constants;
using Filters;
using FluentValidation.AspNetCore;
using Formatters.FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        this.Configuration = configuration;
        this.Environment = environment;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication();

        services.AddInfrastructure(this.Configuration);

        services.AddApplicationMassTransit(this.Configuration, Assembly.GetExecutingAssembly());

        services.AddCustomHealthChecks();

        services
            .AddHostingOptions()
            .AddCustomRouting();

        // Customize default API behavior
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = false);

        services.AddFeatureManagement();

        services
            .AddCustomApiVersioning()
            .AddCustomSwagger(this.Configuration)
            .AddHttpContextAccessor();

        services.AddCustomCors(CorsPolicyName.Default, this.Configuration);

        services.AddAuth(this.Environment, this.Configuration);

        services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilterAttribute>();
            })
            .AddJsonOptions(options =>
            {
                var jsonSerializerOptions = options.JsonSerializerOptions;
                // ! This updates provided JsonOptions.JsonSerializerOptions
                var converterBuilder = services.ConfigureApplicationSerializationOptions(jsonSerializerOptions);
            }).AddFluentValidation(options =>
            {
                options.ValidatorOptions.PropertyNameResolver =
                    CamelCasePropertyNameResolver.ResolvePropertyName;
                options.ValidatorOptions.DisplayNameResolver =
                    SplitPascalCaseDisplayNameResolver.ResolvePropertyName;
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();
        app.UseCustomSerilogRequestLogging();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles()
            .UseDefaultFiles();

        app.UseRouting();

        app.UseCustomSwagger(this.Configuration);

        app.UseCors(CorsPolicyName.Default);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers()
                .RequireCors(CorsPolicyName.Default)
                .RequireAuthorization("CoordinatorAssistantAccess");

            endpoints.MapCustomHealthCheck(
                "/health",
                "/health/ready");
        });
    }
}

#pragma warning restore IDE0058 // Expression value is never used
