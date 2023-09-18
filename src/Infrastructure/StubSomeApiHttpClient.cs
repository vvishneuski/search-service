namespace SearchService.Infrastructure;

using System.Text.Json.Nodes;
using SomeApi.Sdk;

public class StubSomeApiHttpClient : ISomeApiHttpClient
{
    public Task<JsonArray> GetByQueryAsync(string query, CancellationToken cancellationToken) =>
        Task.FromResult(new JsonArray());

    public Task<JsonArray> GetByReverseLookupAsync(string query,
        IEnumerable<string> keys,
        CancellationToken cancellationToken) =>
        Task.FromResult(new JsonArray());
}
