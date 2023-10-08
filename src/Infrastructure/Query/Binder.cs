namespace SearchService.Infrastructure.Query;

using Application.Models;

public class Binder : IBinder
{
    private readonly IEnumerable<IResolver> resolvers;

    public Binder(IEnumerable<IResolver> resolvers) => this.resolvers = resolvers;

    public string Bind(string query, IEnumerable<string> keys, Graph graph)
        => this.resolvers.Aggregate(query,
            (current, resolver) => resolver.Resolve(current, keys, graph));
}
