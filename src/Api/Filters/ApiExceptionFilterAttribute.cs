namespace SearchService.Api.Filters;

using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> exceptionHandlers;

    public ApiExceptionFilterAttribute() =>
        this.exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), this.HandleValidationException },
            { typeof(SearchException), this.HandleSearchException },
            { typeof(UnauthorizedAccessException), this.HandleUnauthorizedAccessException },
        };

    public override void OnException(ExceptionContext context)
    {
        this.HandleException(context);

        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (this.exceptionHandlers.ContainsKey(type))
        {
            this.exceptionHandlers[type].Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
            return;
        }

        HandleUnknownException(context);
    }

    private void HandleValidationException(ExceptionContext context)
    {
        var exception = context.Exception as ValidationException;

        var details = new ValidationProblemDetails(exception?.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private void HandleSearchException(ExceptionContext context)
    {
        var exception = context.Exception as SearchException;

        if (exception.InnerException is ParsingException)
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.InnerException.Message,
                Title = "An error occured during processing of the request",
            };

            context.Result = new BadRequestObjectResult(details);
        }
        else if (exception.InnerException is Exception { Source: "JmePath.Net.Parser" })
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.InnerException.Message,
                Title = "An error occured during parsing of JMESPath. Make sure it is valid",
            };

            context.Result = new BadRequestObjectResult(details);
        }
        else
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3",
                Status = StatusCodes.Status502BadGateway,
                Detail = exception.InnerException.Message,
                Title = "An error occured during sending request to downstream services",
            };

            context.Result = new BadRequestObjectResult(details);
        }

        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status401Unauthorized };

        context.ExceptionHandled = true;
    }

    private static void HandleUnknownException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status500InternalServerError };

        context.ExceptionHandled = true;
    }
}
