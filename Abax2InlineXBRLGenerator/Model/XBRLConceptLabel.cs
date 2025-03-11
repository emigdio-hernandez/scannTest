using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a label for an XBRL item
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLLabel
{
    /// <summary>
    /// The role of the label
    /// </summary>
    [JsonProperty("role")]
    public string Role { get; set; }
    /// <summary>
    /// The language of the label
    /// </summary>
    [JsonProperty("lang")]
    public string Language { get; set; }
    /// <summary>
    /// The text of the label
    /// </summary>
    [JsonProperty("lab")]
    public string Label { get; set; }

    /// <summary>
    /// Initializes a new instance of the XBRLConceptLabel class.
    /// </summary>
    /// <param name="role">The role of the label</param>
    /// <param name="language">The language of the label</param>
    /// <param name="label">The text of the label</param>
    public XBRLLabel(string role, string language, string label)
    {
        Role = role;
        Language = language;
        Label = label;
    }
}
