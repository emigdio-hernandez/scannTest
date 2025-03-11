namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL context.
/// </summary>
public class XBRLContext
{
    /// <summary>
    /// The dimensions of the context.
    /// </summary>
    private readonly IDictionary<string, string?> dimensions = new Dictionary<string, string?>();

    /// <summary>
    /// The IDs of the facts associated with the context.
    /// </summary>
    public IList<string> FactIds { get; set; } = new List<string>();

    /// <summary>
    /// The ID of the context.
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// The ID of the entity.
    /// </summary>
    public string? Entity { get; set; }

    /// <summary>
    /// The period of the context.
    /// </summary>
    public XBRLPeriod? Period { get; set; }

    /// <summary>
    /// The dimensions of the context.
    /// </summary>
    public IDictionary<string, string?> Dimensions
    {
        get
        {
            var newDimensions = new Dictionary<string, string?>();
            foreach (var (key, value) in dimensions)
            {
                newDimensions[key] = value;
            }
            return dimensions;
        }
    }

    /// <summary>
    /// The dimensions of the context.
    /// </summary>
    public void AddDimension(string dimensionId, string? memberId)
    {
        dimensions[dimensionId] = memberId;
    }

    /// <summary>
    /// The hash that identifies the context.
    /// </summary>
    public string Hash
    {
        get
        {
            var otherDimensions = new Dictionary<string, string?>
            {
                {"entity", Entity},
            };
            if (Period != null && Period.PeriodType == Constants.XBRLInstantPeriodType)
            {
                otherDimensions["period"] = Period.PeriodInstantDate;
            }
            else if (Period != null)
            {
                otherDimensions["period"] = Period.PeriodStartDate + "_" + Period.PeriodEndDate;
            }
            foreach (var (key, value) in dimensions)
            {
                otherDimensions[key] = value;
            }
            return XBRLFact.Hash(otherDimensions);
        }
        
    }
}