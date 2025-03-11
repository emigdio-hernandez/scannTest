using System;
using System.Globalization;
using System.Xml;

namespace Abax2InlineXBRLGenerator.Generator.Formatters;
/// <summary>
/// Formats a decimal value with dot as the decimal separator.
/// </summary>
public class NumDotDecimalFormatter : BaseValueFormatter
{
    /// <summary>
    /// Formats the specified value.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>The formatted value</returns>
    public override string Format(string value)
    {
        if (decimal.TryParse(value, out decimal num))
        {
            return num == 0 ? "0" : Math.Abs(num).ToString("#,###", Culture);
        }
        return FormatFailure(value);
    }

    /// <summary>
    /// Gets the additional attributes for the specified value.
    /// </summary>
    /// <param name="value">The value to get the attributes for</param>
    /// <returns>The additional attributes</returns>
    public override IDictionary<string, string>? GetAdditionalAttributes(string? value)
    {
        if (decimal.TryParse(value, out decimal num))
        {
            if (num < 0)
            {
                return new Dictionary<string, string> { { "sign", "-" } };
            }
        }
        return null;
    }
} 