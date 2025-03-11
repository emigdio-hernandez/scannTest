using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;
using System.Text;
using AbaxXBRLCnbvPersistence.Services.Impl;

namespace Abax2InlineXBRLGenerator.Generator;

public class XbrlViewerFactListTagProcessor : TagProcessor
{
    public XbrlViewerFactListTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        var results = new List<XmlNode>();
        
        var scriptElement = Context.Document.CreateElement(prefix: null,
            localName: "script",
            namespaceURI: "http://www.w3.org/1999/xhtml");
        scriptElement.SetAttribute("type", "text/javascript");
        
        // Obtener los FactViewerData ya procesados
        var factsDict = Context.GetFactsViewerData();
        var documentConfig = Context.InstanceDocument.DocumentConfig;
        
        scriptElement.InnerText = $"window.facts = {JsonConvert.SerializeObject(factsDict, Newtonsoft.Json.Formatting.Indented)}; \n" +
                                  $"window.units = {JsonConvert.SerializeObject(Context.InstanceDocument.Units, Newtonsoft.Json.Formatting.Indented)}; \n" +
                                  $"window.documentConfig = {JsonConvert.SerializeObject(documentConfig, Newtonsoft.Json.Formatting.Indented)}; \n";   

        results.Add(scriptElement);
        return results;
    }
    
}

