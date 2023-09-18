namespace SearchService.Application.Interfaces;

public interface IExternalMessageProducer
{
    Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken)
        where T : class;
}
