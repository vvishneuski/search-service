namespace SearchService.Messaging.Contracts;

public record SearchRequest(
    string TransactionId,
    string Request,
    string Mapping);
