namespace SearchService.Messaging.Contracts;

public record DataSetQuery(
    string PrimaryKey,
    string CollectionName,
    string? ParentPath,
    string Query,
    IList<DataSetQuery> Descendants);
