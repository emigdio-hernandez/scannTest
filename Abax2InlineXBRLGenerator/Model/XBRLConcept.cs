namespace Abax2InlineXBRLGenerator.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
/// <summary>
/// Represents an XBRL concept.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLConcept
{
    /// <summary>
    /// The ID of the concept.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// The name of the concept.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The namespace of the concept.
    /// </summary>
    [JsonProperty("ns")]
    public string Namespace { get; set; }
    /// <summary>
    /// The period type of the concept.
    /// </summary>
    [JsonProperty("period")]
    public string PeriodType { get; set; }
    /// <summary>
    /// The balance of the concept.
    /// </summary>
    [JsonProperty("bal")]
    public string? Balance { get; set; }
    /// <summary>
    /// The data type of the concept.
    /// </summary>
    [JsonProperty("type")]
    public string DataType { get; set; }
    /// <summary>
    /// The XBRL data type of the concept.
    /// </summary>
    [JsonProperty("xbrlType")]
    public string XBRLDataType { get; set; }
    /// <summary>
    /// The substitution group of the concept.
    /// </summary>
    [JsonProperty("itemType")]
    public int ItemType { get; set; }
    /// <summary>
    /// A flag indicating if the concept is abstract.
    /// </summary>
    [JsonProperty("abstract")]
    public bool IsAbstract { get; set; }
    /// <summary>
    /// A flag indicating if the concept is nillable.
    /// </summary>
    [JsonProperty("nill")]
    public bool IsNillable { get; set; }
    /// <summary>
    /// When the concept is a dimension member, this property contains the ID of the parent dimension.
    /// </summary>
    [JsonProperty("parentDim")]
    public string? ParentDimension { get; set; }
    /// <summary>
    /// A flag indicating if the concept is a typed dimension.
    /// </summary>
    [JsonProperty("typedDim")]
    public bool IsTypedDimension { get; set; }
    /// <summary>
    /// A flag indicating if the concept is a dimension concept.
    /// </summary>
    [JsonProperty("isDim")]
    public bool IsDimensionConcept { get; set; }
    /// <summary>
    /// A flag indicating if the concept is a dimension member.
    /// </summary>
    [JsonProperty("isDimMember")]
    public bool IsDimensionMember { get; set; }

    /// <summary>
    /// Additional attributes of the concept.
    /// </summary>
    [JsonProperty("attrs")]
    public IDictionary<string, string>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The list of references associated with the concept.
    /// </summary>
    [JsonProperty("refs")]
    public IList<XBRLReference> References { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLConcept"/> class.
    /// </summary>
    /// <param name="id">the id of the concept</param>
    /// <param name="name">the name of the concept</param>
    /// <param name="conceptNamespace">the namespace of the concept</param>
    /// <param name="periodType">the period type of the concept</param>
    /// <param name="balance">the balance of the concept</param>
    /// <param name="dataType">the data type of the concept</param>
    /// <param name="itemType">the substitution group of the concept</param>
    /// <param name="isAbstract">a flag indicating if the concept is abstract</param>
    /// <param name="isNillable">a flag indicating if the concept is nillable</param>
    /// <param name="parentDimension">the id of the parent dimension</param>
    public XBRLConcept(string id, string name, string conceptNamespace, string periodType, string? balance, string dataType, string xbrlDataType, int itemType, bool isAbstract = false, bool isTypedDimension = false, bool isNillable = true, bool isDimensionConcept = false, bool isDimensionMember = false, string? parentDimension = null, IDictionary<string, string>? additionalAttributes = null)
    {
        Id = id;
        Name = name;
        Namespace = conceptNamespace;
        PeriodType = periodType;
        Balance = balance;
        DataType = dataType;
        XBRLDataType = xbrlDataType;
        ItemType = itemType;
        IsAbstract = isAbstract;
        IsNillable = isNillable;
        ParentDimension = parentDimension;
        IsTypedDimension = isTypedDimension;
        IsDimensionConcept = isDimensionConcept;
        IsDimensionMember = isDimensionMember;
        AdditionalAttributes = additionalAttributes;
        References = new List<XBRLReference>();
    }
}