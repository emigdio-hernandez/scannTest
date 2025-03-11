using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-taxonomy-json template tag.
/// Generates a script tag with the taxonomy serialized as JSON.
/// </summary>
public class XbrlTaxonomyJsonTagProcessor : TagProcessor
{
    public XbrlTaxonomyJsonTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        // Crear el elemento script
        var scriptElement = Context.Document.CreateElement(
            prefix: null,
            localName: "script",
            namespaceURI: "http://www.w3.org/1999/xhtml"
        );

        scriptElement.SetAttribute("type", "text/javascript");

        // Crear el contenido del script
        var scriptContent = $@"
            window.xbrlTaxonomy = {JsonConvert.SerializeObject(Context.Taxonomy, Newtonsoft.Json.Formatting.Indented)};
        ";

        // Establecer el contenido del script
        scriptElement.InnerText = scriptContent;

        // Procesar cualquier atributo adicional que pueda tener la etiqueta template
        foreach (XmlAttribute attr in element.Attributes)
        {
            if (!IsReservedAttribute(attr.LocalName))
            {
                scriptElement.SetAttribute(
                    attr.LocalName,
                    Context.EvaluateExpression(attr.Value)
                );
            }
        }

        return new List<XmlNode> { scriptElement };
    }

    private bool IsReservedAttribute(string attributeName)
    {
        // Por ahora no hay atributos reservados para esta etiqueta
        return false;
    }
} 