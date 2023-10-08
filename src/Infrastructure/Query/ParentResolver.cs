namespace SearchService.Infrastructure.Query;

using Application.Models;

public class ParentResolver : IResolver
{
    public string Resolve(string query, IEnumerable<string> keys, Graph graph) =>
        query.Replace("{{parents}}", string.Join(",", keys), StringComparison.OrdinalIgnoreCase);
}
