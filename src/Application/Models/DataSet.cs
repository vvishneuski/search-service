namespace SearchService.Application.Models;

using System.Text.Json.Nodes;
using DevLab.JmesPath;
using Utils;

public sealed class DataSet
{
    private string PrimaryKey { get; }
    public Dictionary<string, JsonNode> Items { get; }

    public DataSet(Func<JsonNode, string, string> primaryKeySelector, string primaryKey, JsonArray data)
    {
        this.PrimaryKey = primaryKey;

        this.Items = data
            .Select(item => item?.Copy())
            .GroupBy(item => primaryKeySelector(item!, this.PrimaryKey))
            .ToDictionary(grouping => grouping.Key!, ObjectOrArray!);
    }

    private static JsonNode ObjectOrArray(IGrouping<string, JsonNode> grouping) =>
        grouping.Count() == 1 ? grouping.Single() : new JsonArray(grouping.ToArray());

    public IEnumerable<JsonNode> GetItems(IEnumerable<string> keys) =>
        keys.Join(this.Items, key => key, _ => _.Key, (key, _) => _.Value.Copy());
}
