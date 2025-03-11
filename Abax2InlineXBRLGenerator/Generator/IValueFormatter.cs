namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// General interface for value formatters.
/// </summary>
public interface IValueFormatter
{
    /// <summary>
    /// Formats the specified value.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>The formatted value</returns>
    string Format(string value);

    /// <summary>
    /// Gets the xbrl format alias for the specified value.
    /// </summary>
    /// <param name="value">The value to get the alias for</param>
    /// <returns>The xbrl format alias</returns>
    string? XbrlFormatAlias(string? value);

    /// <summary>
    /// Gets the additional attributes for the specified value.
    /// </summary>
    /// <param name="value">The value to get the attributes for</param>
    /// <returns>The additional attributes</returns>
    IDictionary<string, string>? GetAdditionalAttributes(string? value);
} 