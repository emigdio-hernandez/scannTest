using System.Globalization;
namespace Abax2InlineXBRLGenerator.Generator.Formatters;
/// <summary>
/// Base class for value formatters.
/// </summary>
public abstract class BaseValueFormatter : IValueFormatter
{
    protected readonly CultureInfo Culture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseValueFormatter"/> class.
    /// </summary>
    /// <param name="culture">The culture to use for formatting. Defaults to invariant culture if not specified.</param>
    protected BaseValueFormatter(CultureInfo? culture = null)
    {
        Culture = culture ?? CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Formats the specified value.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>The formatted value</returns>
    public abstract string Format(string value);

    /// <summary>
    /// Handles the case where the value cannot be formatted.
    /// </summary>
    /// <param name="value">The value that failed to format</param>
    /// <returns>The original value</returns>
    protected virtual string FormatFailure(string value) => value;

    /// <summary>
    /// Gets the xbrl format alias.
    /// </summary>
    /// <param name="value">The value to get the alias for</param>
    /// <returns>The xbrl format alias</returns>
    public virtual string? XbrlFormatAlias(string? value) => null;

    /// <summary>
    /// Gets the additional attributes for the specified value.
    /// </summary>
    /// <param name="value">The value to get the attributes for</param>
    /// <returns>The additional attributes</returns>
    public virtual IDictionary<string, string>? GetAdditionalAttributes(string? value) => null;
} 