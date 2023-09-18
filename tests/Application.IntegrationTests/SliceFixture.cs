namespace SearchService.Application.IntegrationTests;

using Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Ref:
///     https://github.com/jbogard/ContosoUniversityDotNetCore/blob/master/ContosoUniversity.IntegrationTests/SliceFixture.cs
/// </summary>
public class SliceFixture
{
    private static readonly IConfigurationRoot Configuration;
    private static readonly IServiceScopeFactory ScopeFactory;

    static SliceFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables();
        Configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddApplication();
        services.AddInfrastructure(Configuration);

        var provider = services.BuildServiceProvider();
        ScopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using (var scope = ScopeFactory.CreateScope())
        {
            await action(scope.ServiceProvider).ConfigureAwait(false);
        }
    }

    public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using (var scope = ScopeFactory.CreateScope())
        {
            var result = await action(scope.ServiceProvider).ConfigureAwait(false);

            return result;
        }
    }

    public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) =>
        ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetService<IMediator>();

            return mediator.Send(request);
        });

    public static Task SendAsync(IRequest request) =>
        ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetService<IMediator>();

            return mediator.Send(request);
        });
}
