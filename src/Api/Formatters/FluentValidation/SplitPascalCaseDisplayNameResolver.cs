namespace SearchService.Api.Formatters.FluentValidation;

using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// Ref: https://github.com/FluentValidation/FluentValidation/issues/226
public class SplitPascalCaseDisplayNameResolver
{
    public static string ResolvePropertyName(
#pragma warning disable IDE0060 // Remove unused parameter
        Type type, MemberInfo memberInfo, LambdaExpression expression) =>
#pragma warning restore IDE0060 // Remove unused parameter
        SplitPascalCase(memberInfo?.Name);


    /// <summary>
    ///     Splits pascal case, so "FooBar" would become "Foo Bar".
    /// </summary>
    /// <remarks>
    ///     Pascal case strings with periods delimiting the upper case letters,
    ///     such as "Address.Line1", will have the periods removed.
    /// </remarks>
    internal static string SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Reserved buffer space to avoid StringBuilder changing size.
        // The number is based on heuristic.
        var reservedBufferSize = 5;

        var retVal = new StringBuilder(input.Length + reservedBufferSize);

        for (var i = 0; i < input.Length; ++i)
        {
            var currentChar = input[i];
            if (char.IsUpper(currentChar))
            {
                if ((i > 1 && !char.IsUpper(input[i - 1]))
                    || (i + 1 < input.Length && !char.IsUpper(input[i + 1])))
                {
                    retVal.Append(' ');
                }
            }

#pragma warning disable IDE0002 // Simplify Member Access
            if (!Equals('.', currentChar)
                || i + 1 == input.Length
                || !char.IsUpper(input[i + 1]))
            {
                retVal.Append(currentChar);
            }
#pragma warning restore IDE0002 // Simplify Member Access
        }

        return retVal.ToString().Trim();
    }
}
