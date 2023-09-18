namespace SomeApi.Sdk;

using System.Text.Json.Nodes;

public interface ISomeApiHttpClient
{
    Task<JsonArray> GetByQueryAsync(string query, CancellationToken cancellationToken);

    Task<JsonArray> GetByReverseLookupAsync(string query,
        IEnumerable<string> keys,
        CancellationToken cancellationToken);
}
