namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a typed dimension in an inline XBRL document.
/// </summary>
public class XbrlTypedDimension 
{
    /// <summary>
    /// The ID of the typed dimension.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the typed dimension.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The list of members of the typed dimension.
    /// </summary>
    public IList<XbrlTypedDimensionMember> Members { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XbrlTypedDimension"/> class.
    /// </summary>
    /// <param name="id">the id of the typed dimension</param>
    /// <param name="name">the name of the typed dimension</param>
    public XbrlTypedDimension(string id, string name)
    {
        Id = id;
        Name = name;
        Members = new List<XbrlTypedDimensionMember>();
    }
}