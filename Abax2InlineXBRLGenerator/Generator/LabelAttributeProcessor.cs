using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;
namespace Abax2InlineXBRLGenerator.Generator;

public class LabelAttributeProcessor : AttributeProcessor
{
    public override string AttributeName => "xbrl-label";

    public LabelAttributeProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        var concept = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:{AttributeName}");
        var role = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:role");
        role = string.IsNullOrWhiteSpace(role) ? "http://www.xbrl.org/2003/role/label" : role;

        if (string.IsNullOrEmpty(concept))
        {
            throw new TemplateProcessingException($"Attribute '{AttributeName}' must have a value");
        }

        // Crear nuevo elemento con los mismos atributos
        var newElement = Context.Document.CreateElement(
            element.Prefix,
            element.LocalName,
            element.NamespaceURI);

        // Copiar atributos existentes
        foreach (XmlAttribute attr in element.Attributes)
        {
            if (!IsTemplateAttribute(attr))
            {
                var newAttr = Context.Document.CreateAttribute(
                    attr.Prefix,
                    attr.LocalName,
                    attr.NamespaceURI);
                newAttr.Value = Context.EvaluateExpression(attr.Value);
                newElement.Attributes.Append(newAttr);
            }
        }

        var labelDictionary = new Dictionary<string, string> {
            { "data-xbrl-concept", concept },
            { "data-xbrl-role", role },
            { "lang", Context.Configuration.DefaultLanguage }
        };

        newElement.SetAttribute("title", JsonConvert.SerializeObject(labelDictionary));


        // Agregar clase CSS
        var currentClass = GetAttributeValue(element, "class");
        newElement.SetAttribute("class", $"{currentClass} xbrl-label {Configuration.DefaultConceptLabelStyles}".Trim());

        // Establecer el texto de la etiqueta
        if (TryGetInitialLabel(concept, role, out var label))
        {
            newElement.InnerText = label;
        }

        // Procesar hijos si existen
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                newElement.AppendChild(node);
            }
        }

        return new List<XmlNode> { newElement };
    }

    

    private bool TryGetInitialLabel(string conceptId, string role, out string label)
    {
        label = string.Empty;

        if (!Context.Taxonomy.Concepts.TryGetValue(conceptId, out var concept))
        {
            if (Context.Taxonomy.RoleLabels.TryGetValue(conceptId, out var roleLabels))
            {
                if (Context.Configuration.DefaultLanguage == null)
                {
                    return false;
                }
                if (roleLabels.TryGetValue(Context.Configuration.DefaultLanguage, out var defaultLabel))
                {
                    label = defaultLabel.Label;
                    return true;
                }
            }
            return false;
        }

        if (Context.Configuration.DefaultLanguage == null)
        {
            return false;
        }

        var labelInfo = GetConceptLabel(concept, role, Context.Configuration.DefaultLanguage);
        if (labelInfo != null)
        {
            label = labelInfo.Label;
            return true;
        }

        return false;
    }

    private XBRLLabel? GetConceptLabel(XBRLConcept concept, string role, string language)
    {
        return Context.Taxonomy.GetConceptLabel(concept.Id, language, role);
    }
}
