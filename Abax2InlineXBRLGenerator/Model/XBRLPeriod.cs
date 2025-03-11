using System;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL period.
/// </summary>
public class XBRLPeriod
{
    /// <summary>
    /// The ID of the period.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The type of the period.
    /// </summary>
    public string? PeriodType { get; set; }

    /// <summary>
    /// The start date of the period.
    /// </summary>
    public string? PeriodStartDate { get; set; }

    /// <summary>
    /// The end date of the period.
    /// </summary>
    public string? PeriodEndDate { get; set; }

    /// <summary>
    /// The instant date of the period.
    /// </summary>
    public string? PeriodInstantDate { get; set; }
}
