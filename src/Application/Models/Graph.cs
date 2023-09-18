namespace SearchService.Application.Models;

using System.Text.Json.Nodes;
using DevLab.JmesPath;
using Utils;

public sealed class Graph
{
    /// <summary>
    ///     Name of the special field that stores a cross-join key
    /// </summary>
    public const string Key = "_key";

    private Dictionary<string, JsonObject> Nodes { get; }

    public Graph(string collectionName, DataSet dataSet) =>
        this.Nodes = dataSet.Items.ToDictionary(
            _ => _.Key,
            _ => new JsonObject { { Key, _.Key }, { collectionName, _.Value } });

    public JsonArray ToJson() =>
        new(this.Nodes.Values.OfType<JsonNode>().ToArray());


    public void Append(string collectionName, string path, DataSet dataSet)
    {
        foreach (var (key, node) in this.Nodes)
        {
            var keys = GetKeys(node, path).ToList();
            var items = dataSet.GetItems(keys).ToArray();
            if (items.Any())
            {
                node.Add(collectionName, ObjectOrArray(items));
            }
        }
    }

    private static JsonNode ObjectOrArray(JsonNode[] items) =>
        items.Length == 1 ? items.Single() : new JsonArray(items);

    public IEnumerable<string> GetKeys(string path) =>
        this.Nodes.SelectMany(_ => GetKeys(_.Value, path)).Distinct();

    private static IEnumerable<string> GetKeys(JsonNode node, string path)
    {
        var keysJson = new JmesPath().Transform(node.ToJsonString(), path);
        var keys = JsonNode.Parse(keysJson)!;

        return keys switch
        {
            JsonValue key => new[] { Format(key) },
            JsonArray array => array.OfType<JsonValue>().Select(Format).Distinct(),
            _ => Enumerable.Empty<string>()
        };

        string Format(JsonNode key)
        {
            return key.ToJsonString().TrimJsonString();
        }
    }
}
