using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-role-label attribute.
/// </summary>
public class RoleLabelAttributeProcessor : AttributeProcessor
{
    public override string AttributeName => "xbrl-role-label";

    public RoleLabelAttributeProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        var roleUri = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:{AttributeName}");

        if (string.IsNullOrEmpty(roleUri))
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

        // Obtener las etiquetas del rol en todos los idiomas disponibles
        var labels = GetRoleLabels(roleUri);

        // Crear objeto para el título
        var titleObject = new
        {
            roleUri = roleUri,
            labels = labels
        };

        // Establecer el atributo title
        newElement.SetAttribute("title", JsonConvert.SerializeObject(titleObject));

        // Agregar clase CSS
        var currentClass = GetAttributeValue(element, "class");
        newElement.SetAttribute("class", $"{currentClass} xbrl-role-label".Trim());

        // Establecer el texto de la etiqueta en el idioma por defecto
        if (Context.Configuration.DefaultLanguage != null && labels.ContainsKey(Context.Configuration.DefaultLanguage))
        {
            var defaultLabel = labels[Context.Configuration.DefaultLanguage];
            if (!string.IsNullOrEmpty(defaultLabel))
            {
                newElement.InnerText = defaultLabel;
            }
        }
        else
        {
            if (Context.Taxonomy.PresentationLinkbases.TryGetValue(roleUri, out var linkbaseItem))
            {
                newElement.InnerText = linkbaseItem.Name;
            }
        }
        

        // Procesar hijos si existen (aunque probablemente serán reemplazados por la etiqueta)
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                newElement.AppendChild(node);
            }
        }

        return new List<XmlNode> { newElement };
    }


    private Dictionary<string, string> GetRoleLabels(string roleUri)
    {
        var labels = new Dictionary<string, string>();

        // Obtener las etiquetas del rol para todos los idiomas soportados
        if (Context.Taxonomy.RoleLabels.ContainsKey(roleUri))
        {
            foreach (var (language, label) in Context.Taxonomy.RoleLabels[roleUri])
            {
                if (!string.IsNullOrEmpty(label.Label))
                {
                    labels[language] = label.Label;
                }
            }
        }

        return labels;
    }
}