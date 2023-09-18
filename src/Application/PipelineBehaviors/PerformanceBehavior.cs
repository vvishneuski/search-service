namespace SearchService.Application.PipelineBehaviors;

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<TRequest> logger;
    private readonly Stopwatch timer;

    public PerformanceBehavior(ILogger<TRequest> logger)
    {
        this.timer = new Stopwatch();
        this.logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        this.timer.Start();

        var response = await next();

        this.timer.Stop();

        var elapsedMilliseconds = this.timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;

            this.logger.LogWarning(
                "SearchService.Api Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName, elapsedMilliseconds, request);
        }

        return response;
    }
}
