using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL unit.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLUnit
{
    /// <summary>
    /// The ID of the unit.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The description of the unit.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The measure of the unit.
    /// </summary>
    public IList<XBRLUnitMeasure>? Multipliers { get; set; }

    /// <summary>
    /// The type of the unit.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The numerator of the unit.
    /// </summary>
    public IList<XBRLUnitMeasure>? Numerator { get; set; }

    /// <summary>
    /// The denominator of the unit.
    /// </summary>
    public IList<XBRLUnitMeasure>? Denominator { get; set; }

    /// <summary>
    /// The IDs of the facts that use this unit.
    /// </summary>
    public IList<string> FactIds { get; set; } = new List<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLUnitDTO"/> class.
    /// </summary>
    /// <param name="unitId">the id of the unit</param>
    /// <param name="measure">the measure of the unit</param>
    /// <param name="numerator">the numerator of the unit</param>
    /// <param name="denominator">the denominator of the unit</param>
    public XBRLUnit(string? unitId, string? type = null, string? description = null)
    {
        Id = unitId;
        Type = type;
        Numerator = new List<XBRLUnitMeasure>();
        Denominator = new List<XBRLUnitMeasure>();
        Multipliers = new List<XBRLUnitMeasure>();
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLUnitDTO"/> class.
    /// </summary>
    public XBRLUnit()
    {
        Numerator = new List<XBRLUnitMeasure>();
        Denominator = new List<XBRLUnitMeasure>();
        Multipliers = new List<XBRLUnitMeasure>();
    }
}