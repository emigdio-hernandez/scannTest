using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-role-container template tag.
/// </summary>
public class RoleContainerTagProcessor : TagProcessor
{
    private const string AdditionalLocalizedTextAttributeName = "localized-text-to-add";
    public RoleContainerTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        // Obtener atributos requeridos
        var id = GetAttributeValue(element, "id") ?? 
            throw new TemplateProcessingException("Attribute 'id' is required for xbrl-role-container");
        var roleUri = GetAttributeValue(element, "roleUri") ?? 
            throw new TemplateProcessingException("Attribute 'roleUri' is required for xbrl-role-container");
        var value = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:{AdditionalLocalizedTextAttributeName}");

        // Crear el elemento div especificando el namespace XHTML
        var divElement = Context.Document.CreateElement(
            prefix: null,
            localName: "div",
            namespaceURI: "http://www.w3.org/1999/xhtml"
        );
        
        // Copiar el ID
        divElement.SetAttribute("id", id);
        
        // Procesar las clases CSS
        var cssClasses = new HashSet<string>(
            (GetAttributeValue(element, "class") ?? "")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
        );
        
        // Agregar xbrl-role-container si no existe
        cssClasses.Add("xbrl-role-container");
        
        // Establecer las clases combinadas
        divElement.SetAttribute("class", string.Join(" ", cssClasses));

        JObject? additionalLocalizedText = null;
        try
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Parsear el JSON de las traducciones
                additionalLocalizedText = JObject.Parse(value);
            }
        }
        catch (JsonReaderException ex)
        {
            throw new TemplateProcessingException($"Invalid JSON format in {AdditionalLocalizedTextAttributeName}: {value}", ex);
        }

        // Crear el objeto para el título
        var titleObject = new
        {
            roleUri = roleUri,
            labels = GetRoleLabels(roleUri),
            textToAdd = additionalLocalizedText
        };

        // Serializar a JSON y establecer como título
        divElement.SetAttribute("title", JsonConvert.SerializeObject(titleObject, Newtonsoft.Json.Formatting.Indented));

        // Procesar elementos hijos
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                divElement.AppendChild(node);
            }
        }

        return new List<XmlNode> { divElement };
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