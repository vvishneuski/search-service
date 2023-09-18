namespace SearchService.Api.Consumers;

using System.Net.Mime;
using Infrastructure.Messaging;
using MassTransit;

public class SearchRequestConsumerDefinition : ConsumerDefinition<SearchRequestConsumer>
{
    public SearchRequestConsumerDefinition() =>
        this.EndpointName = MessageBrokerConstants.SearchInQueueName;

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SearchRequestConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseRawJsonSerializer();

        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
        {
            rabbit.UseRawJsonDeserializer();
            rabbit.DefaultContentType = new ContentType("application/json");
        }
    }
}
