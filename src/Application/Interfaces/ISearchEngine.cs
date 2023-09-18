namespace SearchService.Application.Interfaces;

using Commands;
using Models;

public interface ISearchEngine
{
    Task<SearchResponse> SearchAsync(SearchCommand request, CancellationToken cancellationToken);
}
