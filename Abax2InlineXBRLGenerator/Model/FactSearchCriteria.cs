using System;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents search criteria for finding XBRL facts
/// </summary>
public class FactSearchCriteria
{
    /// <summary>
    /// The concept ID to search for
    /// </summary>
    public string? ConceptId { get; set; }

    /// <summary>
    /// The periods to search for. Any fact matching any of these periods will be included.
    /// </summary>
    public string[]? Periods { get; set; }

    /// <summary>
    /// The unit to search for
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// The entity to search for
    /// </summary>
    public string? Entity { get; set; }

    /// <summary>
    /// Exact dimension matches required
    /// </summary>
    public Dictionary<string, string>? Dimensions { get; set; }

    /// <summary>
    /// Dimension filters using regular expressions
    /// </summary>
    public Dictionary<string, string>? DimensionFilters { get; set; }

    /// <summary>
    /// Dimensions that should not be present in the facts
    /// </summary>
    public HashSet<string>? ExcludedDimensions { get; set; }

    /// <summary>
    /// Filter expression for the fact value. Use ${fact.value} to reference the fact value in the expression.
    /// Examples:
    /// - "${fact.value} > 1000"
    /// - "${fact.value} != '0'"
    /// - "${fact.value} >= ${someOtherValue}"
    /// </summary>
    public string? ValueFilter { get; set; }

    /// <summary>
    /// Dimension with typed member
    /// </summary>
    public string dimensionWithTypedMember { get; set; }

}

public static class FactSearchCriteriaParser
{
    private static readonly Regex PeriodsRegex = new(
        "\"periods\"\\s*:\\s*\"(\\[.*?\\])\"",
        RegexOptions.Compiled | RegexOptions.Singleline
    );

    public static FactSearchCriteria Parse(string json)
    {
        // Pre-procesar el JSON para corregir el formato de periods
        var processedJson = PeriodsRegex.Replace(json, match =>
        {
            var periodsArray = match.Groups[1].Value;
            return $"\"periods\": {periodsArray}";
        });

        return JsonConvert.DeserializeObject<FactSearchCriteria>(processedJson)!;
    }
}
