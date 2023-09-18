namespace SearchService.Application.Commands;

using MediatR;
using Models;
using Interfaces;

public class SearchCommandHandler
    : IRequestHandler<SearchCommand, SearchResponse>
{
    private readonly ISearchEngine searchEngine;

    public SearchCommandHandler(ISearchEngine searchEngine) =>
        this.searchEngine = searchEngine;

    public async Task<SearchResponse> Handle( SearchCommand request, CancellationToken cancellationToken) =>
        await this.searchEngine.SearchAsync(request, cancellationToken);
}
