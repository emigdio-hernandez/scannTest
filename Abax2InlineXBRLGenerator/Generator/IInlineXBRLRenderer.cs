using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using AbaxXBRLRealTime.Model.JBRL;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Defines the contract for components that render inline XBRL documents from templates
/// using instance document data.
/// </summary>
public interface IInlineXBRLRenderer
{
    /// <summary>
    /// Renders an inline XBRL document using the specified template and instance document.
    /// </summary>
    /// <param name="templateContent">The XML template content to process</param>
    /// <param name="instanceDocument">The XBRL instance document containing the data</param>
    /// <returns>An XmlDocument containing the rendered inline XBRL</returns>
    Task<XmlDocument> RenderInlineXBRL(string templateContent, RealTimeInstanceDocument instanceDocument, string entryPointHref);  

} 