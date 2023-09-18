# search-service

```text
Эндпоины будут отвечать на GET api.some.com/<entity>/v2/search со следующими параметрами:

q — поисковый запрос в формате RSQL (https://github.com/jirutka/rsql-parser). Кроме стандартных операторов мы также поддерживаем оператор =fts= для полнотекстового поиска по строковым полям
from, size — номер и размер страницы
sortBy — comma-separated список полей с опциональным направлением сортировки: sortBy=field1 desc,field2
facets — comma-separated список полей сущности, по которым нужно построить и вернуть фасеты
В ответ будет возвращаться объект с такими полями:

total — общее количество найденных документов
results — текущая страница (массив объектов в том формате, как они лежат в соответствующих топиках)
facets — фасеты, если они были запрошены.
```

## TODO

Implement more advanced search (maxDepth>1)

Potential spec for future example:

```csharp
var searchEngineMock = new SearchEngineMock(
/*lang=json,strict*/ @"[
{
    ""id1"": ""1"",
    ""child1"": ""c1_1""
},
{
    ""id1"": ""2"",
    ""child1"": ""c1_2""
}]");

        searchEngineMock.ReverseLookupWithPayload(
            "ChildCollection1", new string[] { "c1_1", "c1_2" },
/*lang=json,strict*/ @"[
{
    ""id2"": ""c1_1"",
    ""child2"": ""c2_1""
},
{
    ""id2"": ""c1_2""
}]");
        searchEngineMock.ReverseLookupWithPayload(
            "ChildCollection2", new string[] { "c2_1" },
/*lang=json,strict*/ @"[
{
    ""id3"": ""c2_1"",
    ""found"": true
}]");
responsePayload.Should().BeJson(/*lang=json,strict*/ @"
[
    {
        ""RootCollection"": {
            ""id1"": ""1"",
            ""child1"": ""c1_1""
        },
        ""ChildCollection1"": {
            ""ChildCollection1"": {
                ""id2"": ""c1_1"",
                ""child2"": ""c2_1""
            },
            ""ChildCollection2"": {
                ""id3"": ""c2_1"",
                ""found"": true
            },
            ""_key"": ""c1_1""
        },
        ""_key"": ""1""
    },
    {
        ""RootCollection"": {
            ""id1"": ""1"",
            ""child1"": ""c1_1""
        },
        ""ChildCollection1"": {
            ""ChildCollection1"": {
                ""id2"": ""c1_1"",
                ""child2"": ""c2_1""
            },
            ""_key"": ""c1_1""
        },
        ""_key"": ""2""
    }
]
");
searchEngineMock.Mock.Verify(x => x.ExecuteGetRequestAsync(
    It.IsAny<string>(),
    It.IsAny<CancellationToken>()), Times.Once());

searchEngineMock.Mock.Verify(x => x.GetByReverseLookup(
    It.IsAny<string>(),
    It.IsAny<IEnumerable<string>>(),
    It.IsAny<CancellationToken>()), Times.Exactly(x));
```
