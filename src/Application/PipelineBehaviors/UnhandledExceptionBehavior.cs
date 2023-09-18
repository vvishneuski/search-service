namespace SearchService.Application.PipelineBehaviors;

using MediatR;
using Microsoft.Extensions.Logging;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<TRequest> logger;

    public UnhandledExceptionBehavior(ILogger<TRequest> logger) => this.logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            this.logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName,
                request);

            throw;
        }
    }
}
