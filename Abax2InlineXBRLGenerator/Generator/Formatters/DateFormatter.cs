using System;
using System.Globalization;
namespace Abax2InlineXBRLGenerator.Generator.Formatters;
/// <summary>
/// Formats a date value as a string.
/// </summary>
public class DateFormatter : BaseValueFormatter
{
    private readonly string _format;

    public DateFormatter(string format, CultureInfo? culture = null) : base(culture)
    {
        _format = format;
    }

    /// <summary>
    /// Formats the specified value.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>The formatted value</returns>
    public override string Format(string value)
    {
        if (DateTime.TryParse(value, out DateTime date))
        {
            return date.ToString(_format, Culture);
        }
        return FormatFailure(value);
    }
} 