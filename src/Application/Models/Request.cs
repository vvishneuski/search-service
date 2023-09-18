namespace SearchService.Application.Models;

public class Request
{
    public string PrimaryKey { get; set; } = default!;

    public string CollectionName { get; set; } = default!;

    public string ParentPath { get; set; } = default!;

    public string Query { get; set; } = default!;

    public IList<Request> Descendants { get; set; } = new List<Request>();
}
