using System;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Parses XHTML templates and detects special template tags.
/// </summary>
public class TemplateParser
{
    private readonly TemplateConfiguration _configuration;
    private readonly XmlNamespaceManager _namespaceManager;

    /// <summary>
    /// Initializes a new instance of the TemplateParser class.
    /// </summary>
    public TemplateParser(TemplateConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _namespaceManager = new XmlNamespaceManager(new NameTable());
        InitializeNamespaces();
    }

    /// <summary>
    /// Parses a template string and returns a parsed XML document.
    /// </summary>
    public XmlDocument ParseTemplate(string templateContent)
    {
        try
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(templateContent);

            ValidateTemplate(doc);
            return doc;
        }
        catch (XmlException ex)
        {
            throw new TemplateParsingException("Invalid XML in template", ex);
        }
    }

    /// <summary>
    /// Finds all template tags in the document.
    /// </summary>
    public IEnumerable<XmlElement> FindTemplateTags(XmlDocument document)
    {
        var prefix = _configuration.TemplateNamespacePrefix;
        var tags = new List<XmlElement>();

        // Find all supported template tags
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-fact", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-iterate-dimension", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-label", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-variable", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-set-variable", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-if", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-else", document));
        tags.AddRange(FindElementsByTagName($"{prefix}:xbrl-iterate-variable", document));

        return tags;
    }

    /// <summary>
    /// Finds all ix:header sections in the document.
    /// </summary>
    public XmlNodeList FindHeaderSections(XmlDocument document)
    {
        return document.SelectNodes("//ix:header", _namespaceManager)!;
    }

    /// <summary>
    /// Validates the template structure and content.
    /// </summary>
    private void ValidateTemplate(XmlDocument document)
    {
        // Validate document structure
        ValidateDocumentStructure(document);

        // Validate template tags
        foreach (var tag in FindTemplateTags(document))
        {
            ValidateTemplateTag(tag);
        }

        // Validate header sections
        ValidateHeaderSections(document);

        // Validate template tag nesting
        ValidateTagNesting(document);
    }

    private void ValidateDocumentStructure(XmlDocument document)
    {
        // Check for required root element
        if (document.DocumentElement == null)
        {
            throw new TemplateParsingException("Template must have a root element");
        }

        // Verify required namespaces
        var rootElement = document.DocumentElement;
        foreach (var ns in RequiredNamespaces)
        {
            if (!HasNamespace(rootElement, ns))
            {
                throw new TemplateParsingException($"Required namespace missing: {ns}");
            }
        }
    }

    private void ValidateTemplateTag(XmlElement element)
    {
        switch (element.LocalName)
        {
            case "xbrl-fact":
                ValidateFactTag(element);
                break;
            case "xbrl-iterate-dimension":
                ValidateDimensionIteratorTag(element);
                break;
            case "xbrl-label":
                ValidateLabelTag(element);
                break;
            case "xbrl-variable":
                ValidateVariableTag(element);
                break;
            case "xbrl-set-variable":
                ValidateSetVariableTag(element);
                break;
            case "xbrl-presentation-linkbase":
                ValidatePresentationLinkbaseTag(element);
                break;
            case "xbrl-if":
                ValidateIfTag(element);
                break;
            case "xbrl-else":
                ValidateElseTag(element);
                break;
            case "xbrl-iterate-variable":
                ValidateVariableIteratorTag(element);
                break;
        }
    }

    private void ValidatePresentationLinkbaseTag(XmlElement element)
    {
        var presentationLinkbaseTags = FindElementsByTagName($"{_configuration.TemplateNamespacePrefix}:xbrl-presentation-linkbase", element);
        foreach (XmlElement tag in presentationLinkbaseTags)
        {
            var role = tag.GetAttribute("role");
            var name = tag.GetAttribute("name");

            if (string.IsNullOrEmpty(role))
            {
                throw new TemplateParsingException("Presentation linkbase tag must have a role attribute");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new TemplateParsingException("Presentation linkbase tag must have a name attribute");
            }
        }
    }

    private void ValidateHeaderSections(XmlDocument document)
    {
        var headers = FindHeaderSections(document);
        if (headers.Count == 0)
        {
            throw new TemplateParsingException("Template must contain at least one ix:header section");
        }

        foreach (XmlElement header in headers)
        {
            ValidateHeaderSection(header);
        }
    }

    private void ValidateTagNesting(XmlDocument document)
    {
        // Validate if-else pairs
        ValidateIfElsePairs(document);

        // Validate iterator nesting
        ValidateIteratorNesting(document);

        // Check for maximum nesting level
        ValidateNestingDepth(document.DocumentElement!);
    }

    private void ValidateIfElsePairs(XmlDocument document)
    {
        var ifTags = FindElementsByTagName($"{_configuration.TemplateNamespacePrefix}:xbrl-if", document);
        var elseTags = FindElementsByTagName($"{_configuration.TemplateNamespacePrefix}:xbrl-else", document);

        var elseIds = new HashSet<string>();
        foreach (XmlElement elseTag in elseTags)
        {
            var id = elseTag.GetAttribute("id");
            if (string.IsNullOrEmpty(id))
            {
                throw new TemplateParsingException("else tag must have an id attribute");
            }
            if (!elseIds.Add(id))
            {
                throw new TemplateParsingException($"Duplicate else id found: {id}");
            }
        }

        foreach (XmlElement ifTag in ifTags)
        {
            var elseId = ifTag.GetAttribute("else-id");
            if (!string.IsNullOrEmpty(elseId) && !elseIds.Contains(elseId))
            {
                throw new TemplateParsingException($"Referenced else id not found: {elseId}");
            }
        }
    }

    private void ValidateIteratorNesting(XmlDocument document)
    {
        var iteratorTags = FindElementsByTagName($"{_configuration.TemplateNamespacePrefix}:xbrl-iterate-*", document);
        foreach (XmlElement iterator in iteratorTags)
        {
            var parent = iterator.ParentNode as XmlElement;
            while (parent != null)
            {
                if (parent.LocalName.StartsWith("xbrl-iterate-") &&
                    parent.NamespaceURI == iterator.NamespaceURI)
                {
                    ValidateNestedIterators(parent, iterator);
                }
                parent = parent.ParentNode as XmlElement;
            }
        }
    }

    private int ValidateNestingDepth(XmlNode node, int currentDepth = 0)
    {
        if (currentDepth > _configuration.MaxNestingLevel)
        {
            throw new TemplateParsingException($"Maximum nesting level of {_configuration.MaxNestingLevel} exceeded");
        }

        int maxDepth = currentDepth;
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child is XmlElement)
            {
                maxDepth = Math.Max(maxDepth, ValidateNestingDepth(child, currentDepth + 1));
            }
        }

        return maxDepth;
    }

    private void InitializeNamespaces()
    {
        _namespaceManager.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
        _namespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        _namespaceManager.AddNamespace(_configuration.TemplateNamespacePrefix, _configuration.TemplateNamespace);
        _namespaceManager.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        _namespaceManager.AddNamespace("link", "http://www.xbrl.org/2003/linkbase");
        _namespaceManager.AddNamespace("iso4217", "http://www.xbrl.org/2003/iso4217");
        _namespaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
        _namespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        _namespaceManager.AddNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");
        _namespaceManager.AddNamespace("xbrldt", "http://xbrl.org/2005/xbrldt");

        _configuration.NamespaceManager = _namespaceManager;
    }

    private bool HasNamespace(XmlElement element, string namespaceUri)
    {
        foreach (XmlAttribute attr in element.Attributes)
        {
            if (attr.Name.StartsWith("xmlns:") && attr.Value == namespaceUri)
                return true;
            if (attr.Name == "xmlns" && attr.Value == namespaceUri)
                return true;
        }
        return false;
    }

    private IEnumerable<XmlElement> FindElementsByTagName(string tagName, XmlNode node)
    {
        if (tagName.Contains("*"))
        {
            // Para patrones con comodín, necesitamos dividir la búsqueda
            var prefix = tagName.Split(':')[0];
            var localNamePattern = tagName.Split(':')[1];

            // Primero obtenemos todos los elementos con el prefijo correcto
            var allElements = node.SelectNodes($"//{prefix}:*", _namespaceManager);

            // Luego filtramos por el patrón del nombre local
            return allElements!.Cast<XmlElement>()
                .Where(e => MatchesPattern(e.LocalName, localNamePattern));
        }

        // Para nombres exactos, usamos SelectNodes directamente
        return node.SelectNodes($"//{tagName}", _namespaceManager)!.Cast<XmlElement>();
    }

    private bool MatchesPattern(string name, string pattern)
    {
        // Convierte el patrón de estilo glob a regex
        var regex = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(name, regex);
    }

    private readonly string[] RequiredNamespaces = new[]
    {
        "http://www.xbrl.org/2013/inlineXBRL",
        "http://www.w3.org/1999/xhtml"
    };

    #region Tag Validation Methods

    private void ValidateFactTag(XmlElement element)
    {
        // Implement specific validation for fact tags
    }

    private void ValidateDimensionIteratorTag(XmlElement element)
    {
        // Implement specific validation for dimension iterator tags
    }

    private void ValidateLabelTag(XmlElement element)
    {
        // Implement specific validation for label tags
    }

    private void ValidateVariableTag(XmlElement element)
    {
        // Implement specific validation for variable tags
    }

    private void ValidateSetVariableTag(XmlElement element)
    {
        // Implement specific validation for set-variable tags
    }

    private void ValidateIfTag(XmlElement element)
    {
        // Implement specific validation for if tags
    }

    private void ValidateElseTag(XmlElement element)
    {
        // Implement specific validation for else tags
    }

    private void ValidateVariableIteratorTag(XmlElement element)
    {
        // Implement specific validation for variable iterator tags
    }

    private void ValidateHeaderSection(XmlElement header)
    {
        // Implement specific validation for header sections
    }

    private void ValidateNestedIterators(XmlElement parent, XmlElement child)
    {
        // Implement specific validation for nested iterators
    }

    #endregion
}