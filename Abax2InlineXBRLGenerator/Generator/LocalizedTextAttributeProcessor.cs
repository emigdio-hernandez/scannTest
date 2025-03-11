using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:localized-text attribute.
/// </summary>
public class LocalizedTextAttributeProcessor : AttributeProcessor
{
    private static readonly Dictionary<string, JObject> _aliasCache = new();

    public override string AttributeName => "localized-text";

    public LocalizedTextAttributeProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        var value = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:{AttributeName}");
        var alias = GetAttributeValue(element, "alias");
        // Si el valor parece ser un alias, buscar en el caché
        if (!value.StartsWith("{"))
        {
            if (!_aliasCache.ContainsKey(value))
            {
                throw new TemplateProcessingException($"Localized text alias '{value}' not found");
            }

            return new List<XmlNode> { ApplyLocalizedText(element, _aliasCache[value]) };
        }

        try
        {
            // Parsear el JSON de las traducciones
            var translations = JObject.Parse(value);

            // Si tiene un atributo alias, guardar las traducciones en el caché
            if (!string.IsNullOrEmpty(alias))
            {
                _aliasCache[alias] = translations;
                // Remover el atributo alias ya que no es necesario en el output
                element.RemoveAttribute("alias");
            }

            return new List<XmlNode> { ApplyLocalizedText(element, translations) };
        }
        catch (JsonReaderException ex)
        {
            throw new TemplateProcessingException($"Invalid JSON format in localized-text: {value}", ex);
        }
    }

    private XmlNode ApplyLocalizedText(XmlElement element, JObject translations)
    {
        // Verificar que existan traducciones
        if (translations == null || !translations.HasValues)
        {
            throw new TemplateProcessingException("Localized text object is empty or invalid");
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

        // Obtener el idioma por defecto de la configuración
        var defaultLanguage = Context.Configuration.DefaultLanguage ?? "en";

        // Buscar el texto en el idioma por defecto
        if (!translations.TryGetValue(defaultLanguage, out var defaultText))
        {
            throw new TemplateProcessingException($"No translation found for default language '{defaultLanguage}'");
        }

        // Establecer el texto por defecto como contenido del elemento
        newElement.InnerText = defaultText.ToString();

        // Agregar todas las traducciones como atributo title
        newElement.SetAttribute("title", translations.ToString(Newtonsoft.Json.Formatting.None));

        // Agregar o actualizar el atributo class para incluir "localized-text"
        var classAttr = element.GetAttribute("class");
        var classes = !string.IsNullOrEmpty(classAttr) 
            ? new HashSet<string>(classAttr.Split(' '), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!classes.Contains("localized-text"))
        {
            classes.Add("localized-text");
            newElement.SetAttribute("class", string.Join(" ", classes));
        }

        return newElement;
    }

} 