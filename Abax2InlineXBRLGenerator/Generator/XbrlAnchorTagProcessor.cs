using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-anchor template tag.
/// </summary>
public class XbrlAnchorTagProcessor : TagProcessor
{
    public XbrlAnchorTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        var results = new List<XmlNode>();

        // Crear el elemento anchor
        var anchor = Context.ElementBuilder.Document.CreateElement(
            prefix: null,
            localName: "a",
            namespaceURI: "http://www.w3.org/1999/xhtml");

        // Obtener los atributos
        var classValue = GetAttributeValue(element, "class") ?? string.Empty;
        var id = GetAttributeValue(element, "id");

        // Establecer las clases (incluyendo xbrl-anchor)
        anchor.SetAttribute("class", $"{classValue} xbrl-anchor".Trim());

        // Establecer el ID si se proporcionó
        if (!string.IsNullOrEmpty(id))
        {
            anchor.SetAttribute("id", id);
        }

        // Procesar el contenido hijo si existe
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                anchor.AppendChild(node);
            }
        }

        results.Add(anchor);
        return results;
    }
}