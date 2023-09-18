namespace SearchService.Application.PipelineBehaviors;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Utils;
using ValidationException = Exceptions.ValidationException;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> logger;
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        this.validators = validators;
        this.logger = logger;
    }

    public Task<TResponse> Handle(
        TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var context = new ValidationContext<TRequest>(request);
        var typeName = request.GetGenericTypeName();
        this.logger.LogInformation("Validating command {CommandType}", typeName);
        var failures = this.validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            this.logger.LogWarning(
                "Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName,
                request, failures);
            throw new ValidationException(failures);
        }

        return next();
    }
}
