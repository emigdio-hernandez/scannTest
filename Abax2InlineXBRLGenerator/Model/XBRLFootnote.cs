using AbaxXBRLRealTime.Model.JBRL;
using AbaxXBRLRealTime.Shared;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a footnote in an XBRL instance document.
/// </summary>
public class XBRLFootnote
{
    /// <summary>   
    /// The id of the footnote.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The index of the footnote.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The language of the footnote.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The value of the footnote.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The role of the footnote.
    /// </summary>
    public string? TextRole { get; set; }

    /// <summary>
    /// The blob value of the footnote.
    /// </summary>
    public string? BlobValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLFootnote"/> class.
    /// </summary>
    public XBRLFootnote()
    {
        Id = string.Empty;
        Index = 0;
        Language = "en";
    }

    /// <summary>
    /// Creates a footnote from a fact.
    /// </summary>
    /// <param name="fact">The fact to create the footnote from.</param>
    /// <returns>The footnote created from the fact.</returns>
    /// <exception cref="Exception">Thrown if there is an error decoding the HTML entities.</exception>
    public XBRLFootnote(RealTimeFact fact, int index)
    {
        Id = fact.FactId;
        Value = fact.Value;
        BlobValue = fact.BlobValue;
        Index = index;
        if (fact.Dimensions.ContainsKey(JBRLConstants.LanguageDimensionId))
        {
            Language = fact.Dimensions[JBRLConstants.LanguageDimensionId];
        }
        else
        {
            Language = "en";
        }
        if (fact.Dimensions.ContainsKey(JBRLConstants.TextRoleDimensionId))
        {
            TextRole = fact.Dimensions[JBRLConstants.TextRoleDimensionId];
        }
    }
}