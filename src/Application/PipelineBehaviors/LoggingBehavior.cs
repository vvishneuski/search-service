namespace SearchService.Application.PipelineBehaviors;

using MediatR;
using Microsoft.Extensions.Logging;
using Utils;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => this.logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var genericTypeName = request.GetGenericTypeName();
        this.logger.LogInformation("----- Handling command {CommandName} ({@Command})", genericTypeName, request);
        var response = await next();
        this.logger.LogInformation("----- Command {CommandName} handled - response: {@Response}", genericTypeName,
            response);

        return response;
    }
}
