using System;
using Abax2InlineXBRLGenerator.Model;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

/// <summary>
/// Function to get a formatted date from a period.
/// </summary>
public class DateOfPeriodFunction : ITemplateFunction
{
    private readonly TemplateContext _context;

    public string Name => "dateOfPeriod";

    private const string StartDateDateType = "startdate";
    private const string EndDateDateType = "enddate";

    public DateOfPeriodFunction(TemplateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a formatted date from a period.
    /// </summary>
    /// <param name="arguments">
    /// - arguments[0]: Period ID (string)
    /// - arguments[1]: Date format (string)
    /// - arguments[2]: Optional - Which date to get ("startDate" or "endDate", defaults to "endDate")
    /// </param>
    /// <returns>The formatted date string</returns>
    public object Execute(params object[] arguments)
    {
        if (arguments.Length < 2 || arguments.Length > 3)
            throw new ArgumentException("dateOfPeriod requires 2 or 3 arguments: periodId, format, [dateType]");

        var periodId = arguments[0].ToString();
        var format = arguments[1].ToString();
        var dateType = arguments.Length == 3 ? arguments[2].ToString()?.ToLower() : null;

        if (periodId == null)
            throw new ArgumentException("periodId cannot be null");
        if (format == null)
            throw new ArgumentException("format cannot be null");
        
        if (!string.IsNullOrEmpty(dateType) && dateType.ToLower() != StartDateDateType && dateType.ToLower() != EndDateDateType)
            throw new ArgumentException("dateType must be 'startDate' or 'endDate'");

        if (!_context.InstanceDocument.Periods.TryGetValue(periodId, out var period))
            throw new ArgumentException($"Period '{periodId}' not found");

        DateTime date;
        if (period.PeriodType == Constants.XBRLInstantPeriodType)
        {
            date = DateTime.Parse(period.PeriodInstantDate!);
        }
        else
        {
            if (string.IsNullOrEmpty(dateType))
                throw new ArgumentException("dateType must be specified for non-instant periods");

            date = dateType == StartDateDateType
                ? DateTime.Parse(period.PeriodStartDate!) 
                : DateTime.Parse(period.PeriodEndDate!);
        }

        return date.ToString(format);
    }
}