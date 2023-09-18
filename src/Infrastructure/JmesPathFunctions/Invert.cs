namespace SearchService.Infrastructure.JmesPathFunctions;

using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

public class Invert : JmesPathFunction
{
    public Invert() : base("invert", 1) { }

    public override JToken Execute(params JmesPathFunctionArgument[] args)
    {
        var token = args[0].Token;

        return token.Type switch
        {
            JTokenType.Integer => new JValue(-token.ToObject<long>()),
            JTokenType.Float => new JValue(-token.ToObject<double>()),
            JTokenType.Boolean => new JValue(!token.ToObject<bool>()),
            var type => throw new ArgumentException($"Cannot invert argument of type {type}"),
        };
    }
}
