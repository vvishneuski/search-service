namespace SearchService.Application.UnitTests;

using System.Text.Json.Nodes;
using Commands;
using Exceptions;
using Infrastructure;
using Infrastructure.Query;
using Models;
using Moq;
using SomeApi.Sdk;

public class SearchEngineTests
{
    [Fact]
    public async Task EmptyRootDataSet_ReturnsEmptyResponse()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/);

        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload =
            await sut.SearchAsync(
                new SearchCommand
                {
                    Request = new Request
                    {
                        PrimaryKey = "id", Query = "/v1/employees", CollectionName = "employees",
                    }
                }, CancellationToken.None);

        responsePayload.Should().BeJson("[]");
    }

    [Fact]
    public async Task CollisionOfCollectionNamesInRoot_ExceptionThrown()
    {
        var sut = new SearchEngine(new SomeApiHttpClientMock().Object);

        var exceptionAssertion = await FluentActions.Invoking(
                () => sut.SearchAsync(
                    new SearchCommand
                    {
                        Request = new Request
                        {
                            CollectionName = "employees",
                            Descendants = new List<Request> { new Request { CollectionName = "employees", } }
                        }
                    }, CancellationToken.None))
            .Should()
            .ThrowAsync<SearchException>();

        exceptionAssertion.Which.Message
            .Should().StartWith("Unable to perform a search - Make sure that following collection names are unique:");
    }

    [Fact]
    public async Task CollisionOfCollectionNamesInDescendants_ExceptionThrown()
    {
        var sut = new SearchEngine(new SomeApiHttpClientMock().Object);

        var exceptionAssertion = await FluentActions.Invoking(
                () => sut.SearchAsync(
                    new SearchCommand
                    {
                        Request = new Request
                        {
                            CollectionName = "rootEntity",
                            Descendants = new List<Request>
                            {
                                new Request
                                {
                                    CollectionName = "employees",
                                    Descendants = new List<Request>
                                    {
                                        new Request { CollectionName = "employees" }
                                    }
                                }
                            }
                        }
                    }, CancellationToken.None))
            .Should()
            .ThrowAsync<SearchException>();

        exceptionAssertion.Which.Message
            .Should().StartWith("Unable to perform a search - Make sure that following collection names are unique:");
    }

    [Fact]
    public async Task TwoDataSetsWithItems_Returned()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/ @"[
{
    ""user_id"": ""1"",
    ""catalog_item_id"": ""item_1""
},
{
    ""user_id"": ""2"",
    ""catalog_item_id"": ""item_1""
}]");

        SomeApiHttpClientMock.ReverseLookupWithPayload( /*lang=json,strict*/ @"[
{
    ""global_user_id"": ""1"",
    ""timeZoneOffset"": 2
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "user_id",
                    CollectionName = "mentor_request",
                    Query = "/v1/mentor_requests",
                    Descendants = new List<Request>
                    {
                        new Request
                        {
                            PrimaryKey = "global_user_id",
                            ParentPath = "mentor_request.user_id",
                            CollectionName = "employee",
                            Query = "/v1/employee",
                        }
                    }
                }
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""_key"": ""1"",
        ""mentor_request"": {
            ""user_id"": ""1"",
            ""catalog_item_id"": ""item_1""
        },
        ""employee"":{
            ""global_user_id"": ""1"",
            ""timeZoneOffset"": 2
        }
    },
    {
        ""_key"": ""2"",
        ""mentor_request"": {
            ""user_id"": ""2"",
            ""catalog_item_id"": ""item_1""
        }
    }
]
");
    }

    [Fact]
    public async Task TwoDataSetsWithItems_OneToMany_Returned()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/ @"[
{
    ""global_user_id"": ""1"",
    ""mentee_requests"": [
        {
            ""request_id"":""r1"",
            ""preferred_mentor"":""joe""
        },
        {
            ""request_id"":""r2""
        }
    ]}]");
        SomeApiHttpClientMock.ReverseLookupWithPayload( /*lang=json,strict*/ @"[
{
    ""id"": ""r1"",
    ""name"": ""Course1""
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "global_user_id",
                    CollectionName = "employee",
                    Query = "/v1/employees",
                    Descendants = new List<Request>
                    {
                        new Request
                        {
                            PrimaryKey = "id",
                            CollectionName = "mentee_request",
                            ParentPath = "employee.mentee_requests[].request_id",
                            Query = "/v1/mentee_requests"
                        }
                    }
                }
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""_key"": ""1"",
        ""employee"": {
            ""global_user_id"": ""1"",
            ""mentee_requests"": [
                {
                    ""request_id"":""r1"",
                    ""preferred_mentor"":""joe""
                },
                {
                    ""request_id"":""r2""
                }
            ]
        },
        ""mentee_request"": {
                ""id"": ""r1"",
                ""name"": ""Course1""
        }
    }
]
");
    }

    [Fact]
    public async Task TwoDataSetsWithItems_OneToMany_Returned2()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/ @"[
{
    ""global_user_id"": ""1"",
    ""mentee_requests"": [
        {
            ""request_id"":""r1"",
            ""preferred_mentor"":""joe""
        },
        {
            ""request_id"":""r2""
        }
    ]}]");
        SomeApiHttpClientMock.ReverseLookupWithPayload( /*lang=json,strict*/ @"[
{
    ""id"": ""r1"",
    ""name"": ""Course1""
},
{
    ""id"": ""r2"",
    ""name"": ""Course2""
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "global_user_id",
                    CollectionName = "employee",
                    Query = "/v1/employees",
                    Descendants = new List<Request>
                    {
                        new Request
                        {
                            PrimaryKey = "id",
                            CollectionName = "mentee_request",
                            ParentPath = "employee.mentee_requests[].request_id",
                            Query = "/v1/mentee_requests"
                        }
                    }
                }
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""_key"": ""1"",
        ""employee"": {
            ""global_user_id"": ""1"",
            ""mentee_requests"": [
                {
                    ""request_id"":""r1"",
                    ""preferred_mentor"":""joe""
                },
                {
                    ""request_id"":""r2""
                }
            ]
        },
        ""mentee_request"": [
            {
                    ""id"": ""r1"",
                    ""name"": ""Course1""
            },
            {
                    ""id"": ""r2"",
                    ""name"": ""Course2""
            }
        ]
    }
]
");
    }

    [Fact]
    public async Task ThreeLevelOfDepth_Supported()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock(
/*lang=json,strict*/ @"[
{
    ""id1"": ""1"",
    ""child1"": ""c1_1""
}]");

        SomeApiHttpClientMock.ReverseLookupWithPayload(
            "ChildCollection1", new[] { "c1_1" },
/*lang=json,strict*/ @"[
{
    ""id2"": ""c1_1"",
    ""child2"": ""c2_1""
}]");
        SomeApiHttpClientMock.ReverseLookupWithPayload(
            "ChildCollection2", new[] { "c2_1" },
/*lang=json,strict*/ @"[
{
    ""id3"": ""c2_1"",
    ""found"": true
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "id1",
                    CollectionName = "RootCollection",
                    Query = "RootCollection",
                    Descendants = new List<Request>
                    {
                        new Request
                        {
                            PrimaryKey = "id2",
                            ParentPath = "RootCollection.child1",
                            CollectionName = "ChildCollection1",
                            Query = "ChildCollection1",
                            Descendants = new List<Request>
                            {
                                new Request
                                {
                                    PrimaryKey = "id3",
                                    ParentPath = "ChildCollection1.child2",
                                    CollectionName = "ChildCollection2",
                                    Query = "ChildCollection2"
                                }
                            }
                        }
                    }
                }
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""_key"": ""1"",
        ""RootCollection"": {
            ""id1"": ""1"",
            ""child1"": ""c1_1""
        },
        ""ChildCollection1"": {
            ""id2"": ""c1_1"",
            ""child2"": ""c2_1""
        },
        ""ChildCollection2"": {
            ""id3"": ""c2_1"",
            ""found"": true
        }
    }
]
");
    }

    [Fact]
    public async Task TwoDataSetsDescendantIsMergedBasedOnCompositeKey_Returned()
    {
        var SomeApiHttpClientMock = new SomeApiHttpClientMock( /*lang=json,strict*/ @"[
{
    ""user_id"": ""user_id_1"",
    ""board_id"": ""board_id_1""
},
{
    ""user_id"": ""user_id_2"",
    ""board_id"": ""board_id_2""
}]");

        SomeApiHttpClientMock.ReverseLookupWithPayload( /*lang=json,strict*/ @"[
{
    ""key"": ""user_id_1|board_id_1""
}]");
        var sut = new SearchEngine(SomeApiHttpClientMock.Object);

        var responsePayload = await sut.SearchAsync(
            new SearchCommand
            {
                Request = new Request
                {
                    PrimaryKey = "user_id",
                    CollectionName = "dataset1",
                    Query = "/v1/dataset1",
                    Descendants = new List<Request>
                    {
                        new Request
                        {
                            PrimaryKey = "key",
                            ParentPath = "join('|', [dataset1.user_id, dataset1.board_id])",
                            CollectionName = "dataset2",
                            Query = "/v1/dataset2"
                        }
                    }
                }
            }, CancellationToken.None);

        responsePayload.Should().BeJson( /*lang=json,strict*/ @"
[
    {
        ""_key"": ""user_id_1"",
        ""dataset1"": {
            ""user_id"": ""user_id_1"",
            ""board_id"": ""board_id_1""
        },
        ""dataset2"":{
            ""key"": ""user_id_1|board_id_1""
        }
    },
    {
        ""_key"": ""user_id_2"",
        ""dataset1"": {
            ""user_id"": ""user_id_2"",
            ""board_id"": ""board_id_2""
        }
    }
]
");
    }

        [Fact]
    public async Task ResolveParentsPlaceholder_IdsAreSubstituted()
    {
        const string
            rootRequest = "v2/epampersonpublic/search?q=business_email==podkolzzin1996@gmail.com",
            childRequest = "v2/skill-link/search?q=objectIds.id=in=(17,19)&fields=objectIds.id,spokenLanguages,skillName";

        var api = new Mock<ISomeApiHttpClient>();
        api.Setup(x => x.GetByQueryAsync(rootRequest, default))
            .ReturnsAsync((JsonArray)JsonNode.Parse("""
                                                    [
                                                      {
                                                        "global_user_id": 17,
                                                        "business_email": "podkolzzin1996@gmail.com"
                                                      },
                                                      {
                                                        "global_user_id": 19,
                                                        "business_email": "podkolzzin1996@gmail.com"
                                                      }
                                                    ]
                                                    """)!);
        api.Setup(x => x.GetByReverseLookupAsync(childRequest, It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync((JsonArray)JsonNode.Parse("""
                                                    [
                                                        {
                                                            "objectIds":[
                                                                {"id": 17 },
                                                                {"id": 24 }
                                                            ],
                                                            "spokenLanguages":  [ "UA", "EN" ],
                                                            "skillName": ".NET"
                                                        }
                                                    ]
                                                    """)!);

        var sut = new SearchEngine(api.Object, new Binder(new [] { new ParentResolver() }));

        var responsePayload =
            await sut.SearchAsync(
                new SearchCommand
                {
                    Request = new ()
                    {
                        PrimaryKey = "global_user_id",
                        Query = "v2/epampersonpublic/search?q=business_email==podkolzzin1996@gmail.com",
                        CollectionName = "employee",
                        Descendants = new List<Request>()
                        {
                            new()
                            {
                                PrimaryKey = "objectIds[0].id",
                                Query = "v2/skill-link/search?q=objectIds.id=in=({{parents}})&fields=objectIds.id,spokenLanguages,skillName",
                                ParentPath = "employee.global_user_id",
                                CollectionName = "spoken_languages"
                            },
                        }
                    },
                    Mapping = "[0].spoken_languages"
                }, default);



        api.Verify(x => x.GetByQueryAsync(rootRequest, default), Times.Once);
        api.Verify(x => x.GetByReverseLookupAsync(childRequest, It.IsAny<IEnumerable<string>>(), default), Times.Once);

        responsePayload.Should().BeJson("""
                                        {
                                          "objectIds": [
                                            {
                                              "id": 17
                                            },
                                            {
                                              "id": 24
                                            }
                                          ],
                                          "spokenLanguages": [
                                            "UA",
                                            "EN"
                                          ],
                                          "skillName": ".NET"
                                        }
                                        """);
    }
}
