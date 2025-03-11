using System;
using System.Globalization;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

public class ToDoubleFunction : ITemplateFunction
{
    public string Name => "toDouble";

    public object Execute(params object[] arguments)
    {
        if (arguments.Length != 1)
            throw new ArgumentException("toDouble requires exactly 1 argument");

        var value = arguments[0]?.ToString() ?? 
            throw new ArgumentException("value cannot be null");

        if (!double.TryParse(value, out var result))
            throw new ArgumentException($"Cannot convert '{value}' to double");

        return result.ToString(CultureInfo.InvariantCulture);
    }
}