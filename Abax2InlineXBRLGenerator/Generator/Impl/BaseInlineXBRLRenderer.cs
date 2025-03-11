using System.Globalization;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLRealTime.Model.JBRL;

namespace Abax2InlineXBRLGenerator.Generator.Impl;

/// <summary>
/// Base implementation of IInlineXBRLRenderer that provides common functionality
/// for rendering inline XBRL documents.
/// </summary>
public abstract class BaseInlineXBRLRenderer : IInlineXBRLRenderer
{
    protected readonly TemplateConfiguration Configuration;
    protected readonly XBRLTaxonomy Taxonomy;
    
    

    public async Task<XmlDocument> RenderInlineXBRL(string templateContent, RealTimeInstanceDocument instanceDocument, string entryPointHref)
    {

        TemplateProcessor _processor;
        XBRLTaxonomy _taxonomy;
        TemplateConfiguration _configuration;
        IDictionary<string, string> variables  = new Dictionary<string, string>();

        foreach (var variable in instanceDocument.Variables)
        {
            variables.Add(variable.Key.Contains("-") ? variable.Key.Replace("-", "_") : variable.Key, variable.Value);
        }

        var _instanceDocument = new XBRLInstanceDocument(instanceDocument, 
            new XBRLTaxonomy(instanceDocument.TaxonomyId, instanceDocument.Taxonomy.EspacioNombresPrincipal ,
                entryPointHref, 
                instanceDocument.Taxonomy.nombreAbax,"", instanceDocument.Taxonomy), variables);
        _taxonomy = _instanceDocument.Taxonomy;

       
        var factFinder = new XbrlFactFinder(_instanceDocument, _taxonomy.Concepts);
        //fill the Key facts and FAQ and other document configuration
         _configuration = FillDocumentConfig(factFinder, _instanceDocument);
        _processor = new TemplateProcessor(_configuration, factFinder, _taxonomy, _instanceDocument);

        var document = await _processor.ProcessTemplate(templateContent, variables);

        // Asegurar espacio de nombres XHTML
        var root = document?.DocumentElement;
        if (root != null && root.NamespaceURI != "http://www.w3.org/1999/xhtml")
        {
            root.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
        }

        // Asegurar que los elementos <script> tengan una etiqueta de cierre
        var scriptNodes = document?.GetElementsByTagName("script");
        if (scriptNodes != null)
        {
            foreach (XmlNode scriptNode in scriptNodes)
            {
                if (string.IsNullOrWhiteSpace(scriptNode.InnerText))
                {
                    scriptNode.InnerText = ""; // Asegurar contenido vacío para evitar cierre automático
                }
            }
        }

        return document;
    }

    /// <summary>
    /// Fills the document configuration with the necessary information. This method is implemented by the subclasses.
    /// Implementations should provide the specific logic for filling the document configuration.
    /// </summary>
    /// <param name="factFinder">The fact finder to use.</param>
    /// <param name="instanceDocument">The instance document to use.</param>
    /// <returns>The configured TemplateConfiguration object.</returns>
    protected abstract TemplateConfiguration FillDocumentConfig(XbrlFactFinder factFinder, XBRLInstanceDocument instanceDocument);

} 