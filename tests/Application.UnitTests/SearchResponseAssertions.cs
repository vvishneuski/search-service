namespace SearchService.Application.UnitTests;

using System.Text.Json.Nodes;
using FluentAssertions.Primitives;
using Models;
using Utils;

public static class PayloadAssertionsExtensions
{
    public static SearchResponseAssertions Should(this SearchResponse instance) =>
        new(instance);
}

public class SearchResponseAssertions :
    ReferenceTypeAssertions<SearchResponse, SearchResponseAssertions>
{
    public SearchResponseAssertions(SearchResponse instance)
        : base(instance)
    {
    }

    protected override string Identifier => "SomeApiSearchResponse";

    public AndConstraint<SearchResponseAssertions> BeJson(
        string expectedPayload, string because = "", params object[] becauseArgs)
    {
        var response = this.Subject.Response.ToJson().ToJsonString();

        response.Should().Be(
            JsonNode.Parse(expectedPayload)?.ToJsonString(),
            because,
            becauseArgs);

        return new AndConstraint<SearchResponseAssertions>(this);
    }
}
