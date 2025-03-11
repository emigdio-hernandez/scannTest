using System;
using System.Globalization;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

/// <summary>
/// Function to compare two dates. This function is used to compare if 
/// the first date is earlier, equal or later than the second date.
/// Returns:
/// Less than zero –t1 is earlier than t2.
/// Zero –t1 is the same as t2.
/// Greater than zero –t1 is later than t2.
/// </summary>
public class CompareDateFunction : ITemplateFunction
{
    public string Name => "compareDates";

    public object Execute(params object[] arguments)
    {
        if (arguments.Length != 2)
        {
            throw new ArgumentException("CompareDates function requires exactly 2 arguments");
        }

        var date1 = arguments[0]?.ToString() ?? string.Empty;
        var date2 = arguments[1]?.ToString() ?? string.Empty;

        return CompareDates(date1, date2);
    }

    /// <summary>
    /// Compares two dates in string format and returns -1, 0, or 1 based on their comparison.
    /// </summary>
    /// <param name="date1">First date string in standard format</param>
    /// <param name="date2">Second date string in standard format</param>
    /// <returns>
    /// -1: date1 is earlier than date2
    /// 0: dates are equal
    /// 1: date1 is later than date2
    /// </returns>public object Execute(params object[] arguments)
    public static int CompareDates(string date1, string date2)
    {
        if (string.IsNullOrEmpty(date1) || string.IsNullOrEmpty(date2))
        {
            return 0;
        }

        try
        {
            var firstDate = DateTime.Parse(date1, CultureInfo.InvariantCulture);
            var secondDate = DateTime.Parse(date2, CultureInfo.InvariantCulture);
            
            return DateTime.Compare(firstDate, secondDate);
        }
        catch (FormatException)
        {
            // If parsing fails, treat dates as equal
            return 0;
        }
    }
}