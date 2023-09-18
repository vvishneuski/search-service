namespace SearchService.Infrastructure;

using DevLab.JmesPath;
using JmesPathFunctions;

public static class JmesPathMapper
{
    private static JmesPath GetJmesPath()
    {
        var jmesPath = new JmesPath();
        jmesPath.FunctionRepository.Register<TimeDifference>();
        jmesPath.FunctionRepository.Register<Invert>();
        return jmesPath;
    }

    public static string Transform(string json, string mapping) =>
        GetJmesPath().Transform(json, mapping);
}
