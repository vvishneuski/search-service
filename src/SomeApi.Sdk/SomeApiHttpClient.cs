namespace SomeApi.Sdk;

using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

public class SomeApiHttpClient : ISomeApiHttpClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SomeApiHttpClient> logger;

    private const string ResultsField = "results";
    private const string TotalField = "total";


    public SomeApiHttpClient(HttpClient httpClient, ILogger<SomeApiHttpClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task<JsonArray> GetByQueryAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var response = await this.httpClient.GetStringAsync(query, cancellationToken);

        var payload = JsonNode.Parse(response)!;

        var resultsField = payload[ResultsField];
        var totalField = payload[TotalField];

        return payload switch
        {
            JsonArray results => results,
            _ => resultsField switch
            {
                JsonArray results => this.CheckSinglePageScenario(results, totalField, query),
                _ => throw new SomeApiHttpClientException(
                    $"Response doesn't contain (results:JsonArray) and is not (JsonArray). {query}")
            }
        };
    }

    private JsonArray CheckSinglePageScenario(JsonArray results, JsonNode? totalField, string query)
    {
        if (totalField is JsonValue value && (long)value > results.Count)
        {
            this.logger.LogWarning(
                $"DataSet size is greater than page size. Only single-page scenario is supported. {query}");
        }

        return results;
    }

    public async Task<JsonArray> GetByReverseLookupAsync(
        string query,
        IEnumerable<string> keys,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await this.httpClient.GetStringAsync(ComposeQuery(query, keys), cancellationToken);

            var payload = JsonNode.Parse(response)!;

            return payload switch
            {
                JsonArray results => results,
                _ => throw new SomeApiHttpClientException($"Response is not (JsonArray). {query}")
            };
        }
        // TODO: error-prone, what is the difference between 404 entity not found vs 404 path not found?
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return new JsonArray();
        }
    }

    private static string ComposeQuery(string query, IEnumerable<string> keys) =>
        $"{query.TrimEnd('/')}/{string.Join(',', keys)}";
}
