using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a key fact data in an XBRL instance document. 
/// A Key fact is a data that is relevant to the reporting entity and is used to present this key data in the 
/// document cover page.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLInstanceDocumentKeyFact
{
    /// <summary>
    /// The title of the key fact.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// The description of the key fact.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The fact id of the key fact.
    /// </summary>
    public string FactId { get; set; }
    
    /// <summary>
    /// The role uri of the key fact.
    /// </summary>
    public string RoleUri { get; set; }
}