namespace SearchService.Api.Consumers;

using System.Net;
using System.Text.Json;
using Application.Commands;
using Application.Models;
using MassTransit;
using MediatR;
using Messaging.Contracts;
using Polly;
using Polly.Retry;

public class SearchRequestConsumer : IConsumer<SearchRequest>
{
    private readonly IMediator mediator;

    public SearchRequestConsumer(IMediator mediator) => this.mediator = mediator;

    public async Task Consume(ConsumeContext<SearchRequest> context)
    {
        var (transactionId, request, mapping) = context.Message;

        var searchCommand = new SearchCommand
        {
            Request = GetSearchRequest(GetDataSetQuery(request)), Mapping = mapping,
        };

        var retryPolicy = new TransientHttpFailureRetryPolicy<SearchResponse>();

        var searchResponse =
            await retryPolicy.ExecuteAsync(async () => await this.mediator.Send(searchCommand, CancellationToken.None));

        var response = SearchReply.Success(transactionId, searchResponse.Response);

        await context.Send(response, CancellationToken.None);
    }

    private static DataSetQuery GetDataSetQuery(string request)
    {
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };

        return JsonSerializer.Deserialize<DataSetQuery>(request, serializerOptions)!;
    }

    private static Request GetSearchRequest(DataSetQuery query) =>
        new()
        {
            PrimaryKey = query.PrimaryKey,
            CollectionName = query.CollectionName,
            ParentPath = query.ParentPath ?? string.Empty,
            Query = query.Query,
            Descendants = query
                .Descendants
                ?.Select(GetSearchRequest)
                ?.ToList() ?? new List<Request>(),
        };

    private static async Task ReportErrorAsync(
        ConsumeContext<SearchRequest> context,
        string errorCode,
        Exception ex)
    {
        var error = SearchReply.Error(context.Message.TransactionId, errorCode, ex.Message);

        await context.Send(error, CancellationToken.None);
    }

    private class TransientHttpFailureRetryPolicy<T>
    {
        private const int MaxRetryCount = 5;

        private const double DurationBetweenRetries = 1000;

        private readonly AsyncRetryPolicy policy;

        public TransientHttpFailureRetryPolicy() =>
            this.policy = Policy
                .Handle<HttpRequestException>(CheckHttpErrorStatusCode)
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(MaxRetryCount, GetSleepDuration);

        public async Task<T> ExecuteAsync(Func<Task<T>> action) =>
            await this.policy.ExecuteAsync(action);

        private static bool CheckHttpErrorStatusCode(HttpRequestException exception) =>
            IsTransientHttpErrorStatusCode(exception.StatusCode);

        private static bool IsTransientHttpErrorStatusCode(HttpStatusCode? code) =>
            code is
                HttpStatusCode.RequestTimeout or
                HttpStatusCode.BadGateway or
                HttpStatusCode.GatewayTimeout or
                HttpStatusCode.ServiceUnavailable;

        private static TimeSpan GetSleepDuration(int retryCount) =>
            TimeSpan.FromMilliseconds(DurationBetweenRetries * Math.Pow(2, retryCount - 1));
    }
}
