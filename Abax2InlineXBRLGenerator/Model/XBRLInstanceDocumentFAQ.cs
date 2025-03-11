using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a FAQ data in an XBRL instance document. 
/// A FAQ is a data that is relevant to the reporting entity and is used to present this key data in the 
/// document cover page.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLInstanceDocumentFAQ
{
    /// <summary>
    /// The question of the FAQ.
    /// </summary>
    public string Question { get; set; }    

    /// <summary>
    /// The answer of the FAQ.
    /// </summary>
    public string Answer { get; set; }

    /// <summary>
    /// The role uri of the FAQ.
    /// </summary>
    public string RoleUri { get; set; }

    /// <summary>
    /// The fact id of the FAQ.
    /// </summary>
    public string FactId { get; set; }
}