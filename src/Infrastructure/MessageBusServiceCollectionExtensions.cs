namespace SearchService.Infrastructure;

using System.Reflection;
using System.Security.Authentication;
using MassTransit;
using Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using SearchService.Messaging.Contracts;

public static class MessageBusServiceCollectionExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The services with queue services added.</returns>
    public static IServiceCollection AddApplicationMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? assembly = default)
    {
        var options = configuration
            .GetSection(RabbitMQConfiguration.Section)
            .Get<RabbitMQConfiguration>();

        options ??= new RabbitMQConfiguration();

        var connectionString = new Uri(options.ToConnectionString());

        services.AddMassTransit(x =>
        {
            if (assembly is not null)
            {
                x.AddConsumers(assembly);
            }

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                EndpointConvention.Map<SearchReply>(
                    new Uri($"exchange:{MessageBrokerConstants.SearchOutQueueName}"));

                cfg.Host(connectionString, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);

                    if (options.SSL)
                    {
                        h.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
                    }
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddMassTransitHostedService();

        return services;
    }
}
