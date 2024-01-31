namespace SearchService.Infrastructure;

using System.Text.Json;
using Application.Commands;
using Application.Exceptions;
using Application.Interfaces;
using Application.Models;
using Query;
using SomeApi.Sdk;
using CollectionKeysTupleList = IEnumerable<(
    Application.Models.Request collection, IEnumerable<string>
    keys)>;
using JsonNode = System.Text.Json.Nodes.JsonNode;

public class SearchEngine : ISearchEngine
{
    private readonly ISomeApiHttpClient httpClient;
    private readonly IBinder? binder;

    public SearchEngine(ISomeApiHttpClient httpClient, IBinder? binder = null)
    {
        this.httpClient = httpClient;
        this.binder = binder;
    }

    public async Task<SearchResponse> SearchAsync(
        SearchCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var collections = GetCollections(request.Request).Reverse().ToList();
            EnsureCollectionsValid(collections);

            var graph = await this.BuildGraphAsync(collections, cancellationToken);

            return new SearchResponse { Response = ApplyMapping(graph, request.Mapping) };
        }
        catch (Exception ex)
        {
            throw new SearchException($"Unable to perform a search - {ex.Message}", ex);
        }
    }

    private static IEnumerable<Request> GetCollections(Request request) =>
        request.Descendants.SelectMany(GetCollections).Append(request);

    private static void EnsureCollectionsValid(IReadOnlyCollection<Request> collections)
    {
        EnsureUniqueCollectionNames(collections);
        EnsureNotEmptyKeys(collections);
    }

    private static void EnsureUniqueCollectionNames(IEnumerable<Request> collections)
    {
        var violations = collections
            .GroupBy(collection => collection.CollectionName)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (violations.Any())
        {
            throw new ParsingException(
                $"Make sure that following collection names are unique: {string.Join(", ", violations)}");
        }
    }

    private static void EnsureNotEmptyKeys(IEnumerable<Request> collections)
    {
        var violations = collections
            .Where(collection => string.IsNullOrWhiteSpace(collection.PrimaryKey))
            .Select(collection => collection.CollectionName)
            .ToList();

        if (violations.Any())
        {
            throw new ParsingException(
                $"Make sure that following collections have not empty primary key: {string.Join(", ", violations)}");
        }
    }

    private async Task<Graph> BuildGraphAsync(
        IList<Request> collections,
        CancellationToken cancellationToken)
    {
        string PrimaryKeySelector(JsonNode node, string primaryKey)
        {
            var result = JmesPathMapper.Transform(node.ToString(), primaryKey);
            var parse = JsonNode.Parse(result);
            return parse.ToString();
        }

        var rootCollection = collections.First();
        var rootData = await this.httpClient.GetByQueryAsync(rootCollection.Query, cancellationToken);
        var rootDataSet = new DataSet(PrimaryKeySelector, rootCollection.PrimaryKey, rootData);

        var graph = new Graph(rootCollection.CollectionName, rootDataSet);

        foreach (var (collection, keys) in GetNotEmptyCollectionsWithKeys(graph, collections.Skip(1)))
        {
            var boundQuery = this.binder?.Bind(collection.Query, keys, graph) ?? collection.Query;
            var data = await this.httpClient.GetByReverseLookupAsync(boundQuery, keys, cancellationToken);
            var dataSet = new DataSet(PrimaryKeySelector, collection.PrimaryKey, data);

            graph.Append(collection.CollectionName, collection.ParentPath, dataSet);
        }

        return graph;
    }

    private static CollectionKeysTupleList GetNotEmptyCollectionsWithKeys(
        Graph graph, IEnumerable<Request> collections) =>
        collections
            .Select(collection => (collection, keys: graph.GetKeys(collection.ParentPath)))
            .Where(tuple => tuple.keys.Any());

    private static JsonNode ApplyMapping(Graph graph, string mapping)
    {
        if (string.IsNullOrWhiteSpace(mapping))
        {
            return graph.ToJson();
        }

        try
        {
            return JsonNode.Parse(JmesPathMapper.Transform(graph.ToJson().ToJsonString(), mapping))!;
        }
        catch (Exception ex)
        {
            throw new ParsingException($"Unable to apply expression - {mapping}", ex);
        }
    }
}
