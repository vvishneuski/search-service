namespace SearchService.Application.Models;

using Destructurama.Attributed;

public class SearchResponse
{
    [NotLogged]
    public object Response { get; set; } = default!;
}
