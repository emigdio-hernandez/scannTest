using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents an item in the XBRL presentation linkbase.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLPresentationLinkbaseItem
{
    /// <summary>
    /// The concept ID of the item.
    /// </summary>
    [JsonProperty("id")]
    public string? ConceptId { get; set; }

    /// <summary>
    /// The label role of the item.
    /// </summary>
    [JsonProperty("lab")]
    public string? LabelRole { get; set; }

    /// <summary>
    /// The indent level of the item.
    /// </summary>
    [JsonProperty("indent")]
    public int Indentation { get; set; }

    /// <summary>
    /// Whether the item is abstract.
    /// </summary>
    [JsonProperty("abstract")]
    public bool IsAbstract { get; set; }

    /// <summary>
    /// The number of children of the item.
    /// </summary>
    [JsonProperty("children")]
    public int ChildrenCount { get; set; }
}
