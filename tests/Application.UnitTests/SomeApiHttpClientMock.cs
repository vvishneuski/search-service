namespace SearchService.Application.UnitTests;

using System.Text.Json;
using Moq;
using SomeApi.Sdk;
using JsonArray = System.Text.Json.Nodes.JsonArray;
using JsonNode = System.Text.Json.Nodes.JsonNode;

public class SomeApiHttpClientMock
{
    public SomeApiHttpClientMock(string payload = "[]")
    {
        this.Mock = new Mock<ISomeApiHttpClient>(MockBehavior.Strict);
        this.Mock
            .Setup(x => x.GetByQueryAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonNode.Parse(payload) as JsonArray
                          ?? throw new ArgumentException(
                              "Invalid Payload for mocking"));

        this.Object = this.Mock.Object;
    }

    public SomeApiHttpClientMock(object payload)
        : this(JsonSerializer.Serialize(payload))
    {
    }

    public SomeApiHttpClientMock ReverseLookupWithPayload(
        string path, IEnumerable<string> source, string payload)
    {
        this.Mock.Setup(x => x.GetByReverseLookupAsync(
                It.Is<string>(x => x == path),
                It.Is<IEnumerable<string>>(x => !x.Except(source).Any() && x.Any()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonNode.Parse(payload) as JsonArray
                          ?? throw new ArgumentException(
                              "Invalid Payload for mocking"));

        this.Mock.Setup(x => x.GetByReverseLookupAsync(
                It.Is<string>(x => x == path),
                It.Is<IEnumerable<string>>(x => !x.Any()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonNode.Parse("[]") as JsonArray
                          ?? throw new ArgumentException(
                              "Invalid Payload for mocking"));

        return this;
    }

    public SomeApiHttpClientMock ReverseLookupWithPayload(string payload)
    {
        this.Mock.Setup(x => x.GetByReverseLookupAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonNode.Parse(payload) as JsonArray
                          ?? throw new ArgumentException(
                              "Invalid Payload for mocking"));

        return this;
    }

#pragma warning disable CA1720 // Identifier contains type name
    public ISomeApiHttpClient Object { get; }
    public Mock<ISomeApiHttpClient> Mock { get; }
#pragma warning restore CA1720 // Identifier contains type name
}
