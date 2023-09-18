namespace SearchService.Messaging.Contracts;

public record AsyncResponse(string ResponseStatus, string? ErrorCode, string? ErrorMessage);

public enum ResponseStatusEnum
{
    Success = 0,
    Error = 1
}
