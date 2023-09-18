#pragma warning disable IDE0058 // Expression value is never used

namespace SearchService.Infrastructure;

using Application.Interfaces;
using Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Services;
using SomeApi.Sdk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTime, DateTimeService>();
        services.TryAddScoped<IExternalMessageProducer, MassTransitMessageProducer>();

        if (configuration.GetValue<bool>("UseStubSomeApiHttpClient"))
        {
            services.AddScoped<ISomeApiHttpClient, StubSomeApiHttpClient>();
        }
        else
        {
            services.AddSomeApiHttpClient(configuration
                .GetSection(SomeApiHttpClientOptions.Section)
                .Get<SomeApiHttpClientOptions>());
        }

        services.AddScoped<ISearchEngine, SearchEngine>();

        return services;
    }
}
