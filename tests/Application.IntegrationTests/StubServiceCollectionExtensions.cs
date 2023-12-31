namespace SearchService.Application.IntegrationTests;

using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

public static class StubServiceCollectionExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The services.</returns>
    public static IServiceCollection StubApplicationMessageBroker(
        this IServiceCollection services, Assembly assembly = default)
    {
        services.Remove<IBusControl>();
        services.Remove<IBus>();

        services.AddMassTransit(x =>
        {
            if (assembly != null)
            {
                x.AddConsumers(assembly);
            }

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingInMemory((context, cfg) =>
                cfg.ConfigureEndpoints(context));
        });
        services.AddMassTransitHostedService();

        return services;
    }

    private static void Remove<T>(this IServiceCollection services) =>
        services.Remove(
            services.First(descriptor => descriptor.ServiceType == typeof(T)));
}
