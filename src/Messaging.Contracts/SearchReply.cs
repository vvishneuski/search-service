namespace SearchService.Messaging.Contracts;

public record SearchReply : AsyncResponse
{
    public string TransactionId { get; }
    public object? Result { get; }

    private SearchReply(
        string transactionId,
        object? result,
        string status,
        string? errorCode,
        string? errorMessage)
        : base(status, errorCode, errorMessage)
    {
        this.TransactionId = transactionId;
        this.Result = result;
    }

    public static SearchReply Success(string transactionId, object result) =>
        new(transactionId, result, WriteStatus(ResponseStatusEnum.Success), default, default);

    public static SearchReply Error(string transactionId, string code, string message) =>
        new(transactionId, null, WriteStatus(ResponseStatusEnum.Error), code, message);

    private static string WriteStatus(ResponseStatusEnum status) => status switch
    {
        ResponseStatusEnum.Success => nameof(ResponseStatusEnum.Success).ToUpperInvariant(),
        _ => nameof(ResponseStatusEnum.Error).ToUpperInvariant(),
    };
}
