namespace SomeApi.Sdk;

using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddSomeApiHttpClient(
        this IServiceCollection services, SomeApiHttpClientOptions options)
    {
        void configureSomeApiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(options.BaseUrl);

            if (!string.IsNullOrWhiteSpace(options.XApiKey))
            {
                httpClient.DefaultRequestHeaders.Add(
                    "X-Api-Key",
                    options.XApiKey);
            }
        }

        var builder = services
            .AddHttpClient<ISomeApiHttpClient, SomeApiHttpClient>(configureSomeApiClient);

        AddAuth(services, builder, options);

        return builder;
    }

    private static void AddAuth(
        IServiceCollection services,
        IHttpClientBuilder builder,
        SomeApiHttpClientOptions options)
    {
        var SomeApiHttpClientName = "api-client";
        var tokenExchangeHttpClientName = "token-exchange";

        if (options.AuthDisabled)
        {
            return;
        }

        builder.AddClientAccessTokenHandler(SomeApiHttpClientName)
            .AddHttpMessageHandler(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(tokenExchangeHttpClientName);

                var logger = sp.GetRequiredService<ILogger<TokenExchangeDelegateHandler>>();

                return new TokenExchangeDelegateHandler(httpClient, options, logger);
            });

        services.AddHttpClient(tokenExchangeHttpClientName);

        services.AddAccessTokenManagement(cfg =>
        {
            cfg.Client.Clients.Add(SomeApiHttpClientName,
                new ClientCredentialsTokenRequest
                {
                    Address = options.TokenEndpointUrl,
                    ClientId = options.ClientId,
                    ClientSecret = options.ClientSecret,
                });
        });
    }
}
