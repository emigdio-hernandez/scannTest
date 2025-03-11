
namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL fact dimension.
/// </summary>
public class XBRLFactDimension
{
    /// <summary>
    /// The ID of the dimension.
    /// </summary>
    public string DimensionId { get; set; }
    /// <summary>
    /// The ID of the member.
    /// </summary>
    public string MemberId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLFactDimensionDTO"/> class.
    /// </summary>
    /// <param name="dimensionId">the id of the dimension</param>
    /// <param name="memberId">the id of the member</param>
    public XBRLFactDimension(string dimensionId, string memberId)
    {
        DimensionId = dimensionId;
        MemberId = memberId;
    }
}
