namespace SearchService.Application.UnitTests;

using System.Text.Json.Nodes;
using Commands;
using Infrastructure;
using Models;
using Utils;

public class SearchEnginePostProcessingTests
{
    [Fact]
    public async Task DataSetWithPostProcessing_Returned()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/ @"[
{
    ""user_id"": ""1"",
    ""catalog_item_id"": ""item_1""
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "user_id", Query = "/v1/mentor_requests", CollectionName = "mentor_request"
                },
                Mapping = "[].{ user_global_id: @.mentor_request.user_id }"
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""user_global_id"": ""1""
    }
]");
    }

    [Fact]
    public async Task TryOrderByTimeZoneProximityThenByTrackAndLevel_Sorted()
    {
        var employees = Generate(25).ToList();

        var SomeApiHttpClientMock = new SomeApiHttpClientMock(employees);

        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var selectedEmployee = employees.First();
        var sortByLevel = "sort_by(@, &employees.Level)";
        var sortByTimeZone = $"&abs(sum([employees.TimeZoneOffset, `{selectedEmployee.TimeZoneOffset * -1}`]))";
        var sortByTimeZoneThanByLevel = $"sort_by({sortByLevel}, {sortByTimeZone})";
        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request { PrimaryKey = "Id", Query = "/v1/employees", CollectionName = "employees", },
                Mapping = sortByTimeZoneThanByLevel,
            }, CancellationToken.None);

        var response = responsePayload.Response.ToJson();

        var expectedOrderOfEmployees = employees
            .OrderBy(e => Math.Abs(
                e.TimeZoneOffset - selectedEmployee.TimeZoneOffset))
            .ThenBy(e => e.Level)
            .ToList();

        var responseOrder = (response as JsonArray)!.Select(e => e![Graph.Key]!.ToString());

        responseOrder.Should().ContainInOrder(
            expectedOrderOfEmployees.Select(e => e.Id.ToString()));
    }

    private record Employee(Guid Id, int TimeZoneOffset, int Level);

    private static IEnumerable<Employee> Generate(int count = 5)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new(
                Guid.NewGuid(),
                Random.Shared.Next(-5, 5),
                Random.Shared.Next(1, 3));
        }
    }
}
