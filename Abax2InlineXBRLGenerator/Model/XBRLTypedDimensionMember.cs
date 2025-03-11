namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a typed dimension member in an inline XBRL document.
/// </summary>
public class XbrlTypedDimensionMember 
{
    /// <summary>
    /// The ID of the typed dimension member.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the typed dimension member.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}