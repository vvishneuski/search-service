namespace SearchService.Infrastructure.Query;

using Application.Models;

public interface IResolver
{
    string Resolve(string query, IEnumerable<string> keys, Graph graph);
}
