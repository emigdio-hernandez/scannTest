using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-footnotes-container template tag.
/// </summary>
public class XbrlFootnotesContainerTagProcessor : TagProcessor
{
    public XbrlFootnotesContainerTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        var results = new List<XmlNode>();

        // Crear el contenedor div
        var container = Context.Document.CreateElement(
            prefix: null,
            localName: "div",
            namespaceURI: "http://www.w3.org/1999/xhtml"
        );
        
        // Obtener los atributos
        var id = GetAttributeValue(element, "id");

        var cssClasses = new HashSet<string>(
            (GetAttributeValue(element, "class") ?? "")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
        );
        
        // Agregar xbrl-footnotes-container si no existe
        cssClasses.Add("xbrl-footnotes-container");
        
        // Establecer las clases combinadas
        container.SetAttribute("class", string.Join(" ", cssClasses));

        // Establecer el ID si se proporcionó
        if (!string.IsNullOrEmpty(id))
        {
            container.SetAttribute("id", id);
        }

        // Procesar el contenido hijo si existe
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                container.AppendChild(node);
            }
        }

        results.Add(container);
        return results;
    }
} 