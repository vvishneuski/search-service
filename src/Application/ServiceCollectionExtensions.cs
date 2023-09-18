namespace SearchService.Application;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PipelineBehaviors;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }

    public static IServiceCollection AddAutoMapper<T1>(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(T1).Assembly);

        return services;
    }

    public static IServiceCollection ConfigureApplicationSerializationOptions
        (this IServiceCollection services, JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        return services;
    }
}
