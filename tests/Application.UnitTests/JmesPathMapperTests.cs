namespace SearchService.Application.UnitTests;

using Infrastructure;

public class JmesPathMapperTests
{
    [Theory]
    [InlineData(
        "[0].availia_mentor_participation.workload < `3` && time_difference('days', '2023-03-11 22:31:49Z', [0].availia_mentor_participation.last_assigned_at) > `3`",
        "false")]
    [InlineData(
        "[0].availia_mentor_participation.workload < `3` && time_difference('hours', '2023-03-11 22:31:49Z', [0].availia_mentor_participation.last_assigned_at) > `3`",
        "true")]
    [InlineData(
        "[0].availia_mentor_participation.workload < `3` && time_difference('minutes', '2023-03-11 22:31:49Z', [0].availia_mentor_participation.last_assigned_at) > `3`",
        "true")]
    public void TimeDifference_To_Compare_DateTime(string jmesPathExpression, string expectedValue)
    {
        var jsonData = /*lang=json,strict*/
            "[{\"availia_mentor_participation\":{\"workload\":2,\"last_assigned_at\":\"2023-03-10 22:31:49Z\"}}]";
        JmesPathMapper.Transform(jsonData, jmesPathExpression).Should().Be(expectedValue);
    }

    [Fact]
    public void TimeDifference_To_Compare_DateTime_Throws_Exception()
    {
        var jsonData = /*lang=json,strict*/
            "[{\"availia_mentor_participation\":{\"workload\":2,\"last_assigned_at\":\"2023-03-10 22:31:49Z\"}}]";
        var jmesPathExpression =
            "[0].availia_mentor_participation.workload < `3` && time_difference('months', '2023-03-11 22:31:49Z', [0].availia_mentor_participation.last_assigned_at) > `3`";

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JmesPathMapper.Transform(jsonData, jmesPathExpression)
        );
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "-1")]
    [InlineData(-1, "1")]
    [InlineData(1.1, "-1.1")]
    [InlineData(-1.1, "1.1")]
    [InlineData(-2147483647, "2147483647")]
    [InlineData(2147483647, "-2147483647")]
    [InlineData(true, "false")]
    [InlineData(false, "true")]
    public void Invert_Things(string input, string result) =>
        JmesPathMapper.Transform( /*lang=json,strict*/@$"{{""valueToNegate"": {input.ToLowerInvariant()}}}",
                "invert(valueToNegate)")
            .Should().Be(result);

    [Fact]
    public async Task Invert_NonRevertable_Throws_Exception() =>
        Assert.Throws<ArgumentException>(() =>
            JmesPathMapper.Transform(
                /*lang=json,strict*/"{\"valueToNegate\": {\"object\":1}}",
                "invert(valueToNegate)")
        );
}
