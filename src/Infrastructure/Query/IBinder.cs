namespace SearchService.Infrastructure.Query;

using Application.Models;

public interface IBinder
{
    string Bind(string query, IEnumerable<string> keys, Graph graph);
}
