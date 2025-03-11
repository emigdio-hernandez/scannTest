namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Data of a presentation linkbase in an XBRL taxonomy.
/// </summary>
public class XBRLPresentationLinkbase
{
    public XBRLPresentationLinkbase(string name, string uri)
    {
        PresentationLinkbaseItems = new List<XBRLPresentationLinkbaseItem>();
        Name = name;
        Uri = uri;
    }
    
    /// <summary>
    /// Name of the definition of the linkbase
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Linkbase role URI
    /// </summary>
    public string Uri { get; set; }
    /// <summary>
    /// Structure of presentation linkbase items
    /// </summary>
    public List<XBRLPresentationLinkbaseItem> PresentationLinkbaseItems{get; set;}
}