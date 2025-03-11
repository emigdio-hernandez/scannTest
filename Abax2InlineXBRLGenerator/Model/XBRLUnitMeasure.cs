using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL unit measure.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLUnitMeasure
{
    /// <summary>
    /// Measure identifier <name space>:<name>
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Measure name space.
    /// </summary>
    public string? NameSpace { get; set; }
    /// <summary>
    /// Name of the measure.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLUnitMeasure"/> class.
    /// </summary>
    /// <param name="id">the id of the unit</param>
    /// <param name="nameSpace">the measure of the unit</param>
    /// <param name="name">the name of the unit</param>
    public XBRLUnitMeasure(string? id = null, string? nameSpace = null, string? name = null)
    {
        Id = id;
        NameSpace = nameSpace;
        Name = name;
    }
}
