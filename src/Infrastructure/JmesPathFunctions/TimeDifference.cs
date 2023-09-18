namespace SearchService.Infrastructure.JmesPathFunctions;

using System.Globalization;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

public class TimeDifference : JmesPathFunction
{
    public TimeDifference() : base("time_difference", 3)
    {
    }

    public override JToken Execute(params JmesPathFunctionArgument[] args)
    {
        var minuend = DateTime.ParseExact(
            args[1].Token.ToString(),
            new[] { "o", "u", "s" },
            CultureInfo.InvariantCulture);

        var subtrahend = DateTime.ParseExact(
            args[2].Token.ToString(),
            new[] { "o", "u", "s" },
            CultureInfo.InvariantCulture);

        var difference = minuend - subtrahend;

        var units = args[0].Token.ToString();

        var total = units switch
        {
            "days" => difference.TotalDays,
            "hours" => difference.TotalHours,
            "minutes" => difference.TotalMinutes,
            _ => throw new ArgumentOutOfRangeException(nameof(units), units,
                "allowed values 'days', 'hours' and 'minutes'")
        };

        return new JValue(total);
    }
}
