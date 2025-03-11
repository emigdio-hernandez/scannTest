using Abax2InlineXBRLGenerator.Model;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

/// <summary>
/// Function to find periods that match the specified period based on the start or end date.
/// </summary>

public class MatchPeriodFunction : ITemplateFunction
{
    private readonly TemplateContext _context;

    public string Name => "matchPeriod";

    public MatchPeriodFunction(TemplateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes the match period function which matches periods based on the given period ID and match type.
    /// </summary>
    /// <param name="arguments">
    /// An array of objects where:
    /// - arguments[0] is the period ID (string).
    /// - arguments[1] is the match type (string) which can be either "start" or "end".
    /// - arguments[2] (optional) is the type of periods to match (string) which can be either "instant" or "duration".
    /// </param>
    /// <returns>
    /// A JSON serialized string containing a list of matching period IDs.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of arguments is not 2 or 3, or when the period ID is null,
    /// or when the match type is not "start" or "end", or when typeOfPeriodToMatch is not "instant" or "duration".
    /// </exception>
    public object Execute(params object[] arguments)
    {
        if (arguments.Length < 2 || arguments.Length > 3)
            throw new ArgumentException("matchPeriod requires 2 or 3 arguments");

        var periodId = arguments[0].ToString();
        var matchType = arguments[1].ToString()!.ToLower();
        var typeOfPeriodToMatch = arguments.Length == 3 ? arguments[2].ToString()!.ToLower() : null;

        if (periodId == null)
            throw new ArgumentException("matchPeriod requires a periodId as the first argument");

        if (matchType != "start" && matchType != "end")
            throw new ArgumentException("matchPeriod requires a matchType of 'start' or 'end' as the second argument");

        if (typeOfPeriodToMatch != null && typeOfPeriodToMatch != Constants.XBRLInstantPeriodType && typeOfPeriodToMatch != Constants.XBRLDurationPeriodType)
            throw new ArgumentException("typeOfPeriodToMatch must be either 'instant' or 'duration'");

        var period = _context.InstanceDocument.Periods[periodId];
        var matchingPeriods = new List<string> { periodId }; // Incluir el periodo de referencia

        foreach (var (id, otherPeriod) in _context.InstanceDocument.Periods)
        {
            if (id == periodId) continue; // Skip the reference period since it's already included

            // Si se especificó un tipo de periodo, verificar que coincida
            if (typeOfPeriodToMatch != null && otherPeriod.PeriodType != typeOfPeriodToMatch)
                continue;

            if (period.PeriodType == Constants.XBRLInstantPeriodType)
            {
                if (otherPeriod.PeriodType == Constants.XBRLInstantPeriodType &&
                    period.PeriodInstantDate == otherPeriod.PeriodInstantDate)
                {
                    matchingPeriods.Add(id);
                }
                else if (otherPeriod.PeriodType == Constants.XBRLDurationPeriodType && typeOfPeriodToMatch != Constants.XBRLInstantPeriodType)
                {
                    if (matchType == "end" &&
                        period.PeriodInstantDate == otherPeriod.PeriodEndDate)
                    {
                        matchingPeriods.Add(id);
                    }
                    else if (matchType == "start")
                    {
                        if (DateTime.TryParse(period.PeriodInstantDate, out DateTime instantDate))
                        {
                            var nextDay = instantDate.AddDays(1).ToString("yyyy-MM-dd");
                            if (nextDay == otherPeriod.PeriodStartDate)
                            {
                                matchingPeriods.Add(id);
                            }
                        }
                    }
                }
            }
            else if (period.PeriodType == Constants.XBRLDurationPeriodType)
            {
                if (otherPeriod.PeriodType == Constants.XBRLDurationPeriodType)
                {
                    if (matchType == "start" &&
                        period.PeriodStartDate == otherPeriod.PeriodStartDate)
                    {
                        matchingPeriods.Add(id);
                    }
                    else if (matchType == "end" &&
                        period.PeriodEndDate == otherPeriod.PeriodEndDate)
                    {
                        matchingPeriods.Add(id);
                    }
                }
                else if (otherPeriod.PeriodType == Constants.XBRLInstantPeriodType && typeOfPeriodToMatch != Constants.XBRLDurationPeriodType)
                {
                    if (matchType == "start")
                    {
                        if (DateTime.TryParse(period.PeriodStartDate, out DateTime periodStartDate))
                        {
                            var previousDay = periodStartDate.AddDays(-1).ToString("yyyy-MM-dd");
                            if (previousDay == otherPeriod.PeriodInstantDate)
                            {
                                matchingPeriods.Add(id);
                            }
                        }
                    }
                    else if (matchType == "end" &&
                        period.PeriodEndDate == otherPeriod.PeriodInstantDate)
                    {
                        matchingPeriods.Add(id);
                    }
                }
            }
        }

        return JsonConvert.SerializeObject(matchingPeriods);
    }
}


