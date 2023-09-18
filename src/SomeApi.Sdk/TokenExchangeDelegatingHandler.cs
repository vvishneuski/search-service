namespace SomeApi.Sdk;

using IdentityModel.Client;
using Microsoft.Extensions.Logging;

public class TokenExchangeDelegateHandler : DelegatingHandler
{
    private readonly HttpClient httpClient;
    private readonly SomeApiHttpClientOptions options;
    private readonly ILogger<TokenExchangeDelegateHandler> logger;
    private string? accessToken;

    public TokenExchangeDelegateHandler(
        HttpClient httpClient,
        SomeApiHttpClientOptions options,
        ILogger<TokenExchangeDelegateHandler> logger)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestToken = request.Headers.Authorization?.Parameter;

        var newToken = await this.GetToken(requestToken);

        request.SetBearerToken(newToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetToken(string? requestToken)
    {
        if (!string.IsNullOrEmpty(this.accessToken))
        {
            return this.accessToken;
        }

        var tokenExchangeTokenRequest = new TokenExchangeTokenRequest
        {
            Address = this.options.TokenEndpointUrl,
            ClientId = this.options.ClientId,
            ClientSecret = this.options.ClientSecret,
            Audience = this.options.TargetAudience,
            GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
            SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token",
            SubjectToken = requestToken,
            Method = HttpMethod.Post
        };

        var tokenResponse = await this.httpClient
            .RequestTokenExchangeTokenAsync(tokenExchangeTokenRequest);

        if (tokenResponse.IsError)
        {
            this.logger.LogError($"Error during token retrieval for {this.options.ClientId} - {tokenResponse.Raw}");

            throw new SomeApiHttpClientException(
                $"Unable to retrieve an access token. {tokenResponse.Error}", tokenResponse.Exception);
        }

        this.accessToken = tokenResponse.AccessToken;

        return this.accessToken;
    }
}
