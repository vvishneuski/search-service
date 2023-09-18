namespace SearchService.Application.Utils;

using System.Text.Json;
using JsonNode = System.Text.Json.Nodes.JsonNode;

public static class JsonUtils
{
    public static JsonNode ToJson(this object obj) => JsonNode.Parse(JsonSerializer.Serialize(obj))!;

    public static JsonNode Copy(this JsonNode node) => JsonNode.Parse(node.ToJsonString())!;

    public static string TrimJsonString(this string str) => str.Trim('\'').Trim('\"');
}
