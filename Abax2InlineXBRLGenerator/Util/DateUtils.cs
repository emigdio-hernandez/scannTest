using System.Globalization;
using Abax2InlineXBRLGenerator.Model;
namespace Abax2InlineXBRLGenerator.Util;

public class DateUtils
{
    /// <summary>
    /// Converts the specified date to a string in the format "yyyy-MM-dd".
    /// </summary>
    /// <param name="date">the date to convert</param>
    /// <returns>the date as a string</returns>
    public static string ToDateString(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Parses the specified date string in the format "yyyy-MM-dd".
    /// </summary>
    /// <param name="instantDate"></param>
    /// <returns></returns>
    public static DateTime? ParseInstantDate(string instantDate)
    {
        DateTime periodEndDate;
        if (DateTime.TryParseExact(instantDate, Constants.XBRLPeriodDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out periodEndDate))
        {
            return periodEndDate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Parses the specified duration date span in the format "yyyy-MM-dd_yyyy-MM-dd".
    /// </summary>
    /// <param name="durationDateSpan">the duration date span to parse</param>
    /// <returns>the start and end date of the duration date span</returns>
    public static (DateTime?, DateTime?) ParseDurationDateSpan(string durationDateSpan)
    {
        var dateSpanParts = durationDateSpan.Split('_');
        if (dateSpanParts.Length != 2)
        {
            return (null, null);
        }

        DateTime startDate;
        DateTime endDate;

        if (DateTime.TryParseExact(dateSpanParts[0], Constants.XBRLPeriodDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) &&
            DateTime.TryParseExact(dateSpanParts[1], Constants.XBRLPeriodDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
        {
            return (startDate, endDate);
        }
        else
        {
            return (null, null);
        }
    }
}