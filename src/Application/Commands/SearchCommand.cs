namespace SearchService.Application.Commands;

using MediatR;
using Models;

public class SearchCommand : IRequest<SearchResponse>
{
    public Request Request { get; init; } = default!;

    /// <summary>
    ///     JMESPath query
    /// </summary>
    /// <seealso href="https://jmespath.org/" />
    public string Mapping { get; init; } = string.Empty;
}
