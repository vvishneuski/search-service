namespace SearchService.Infrastructure.Messaging;

using Application.Interfaces;
using MassTransit;

public class MassTransitMessageProducer : IExternalMessageProducer
{
    private readonly IPublishEndpoint endpoint;

    public MassTransitMessageProducer(IPublishEndpoint endpoint) =>
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    //await endpoint.Send(message, cancellationToken);
    public async Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken)
        where T : class =>
        await this.endpoint.Publish(message, cancellationToken);
}
