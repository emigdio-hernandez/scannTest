using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-label template tag.
/// </summary>
public class LabelTagProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the LabelTagProcessor class.
    /// </summary>
    public LabelTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-label tag and returns a span element with the appropriate attributes.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        var concept = GetAttributeValue(element, "concept");
        var dimension = GetAttributeValue(element, "dimension");
        var member = GetAttributeValue(element, "member");
        var role = GetAttributeValue(element, "role");

        var results = new List<XmlNode>();

        if (string.IsNullOrEmpty(role))
        {
            role = "http://www.xbrl.org/2003/role/label";
        }

        // At least one of concept, dimension, or member must be specified
        if (string.IsNullOrEmpty(concept) && string.IsNullOrEmpty(dimension) && string.IsNullOrEmpty(member))
        {
            throw new TemplateProcessingException("At least one of 'concept', 'dimension', or 'member' attributes must be specified");
        }

        // Create span element
        var spanElement = CreateLabelSpanElement(concept, dimension, member, role);

        // Process any child elements
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                spanElement.AppendChild(node);
            }
        }

        results.Add(spanElement);

        return results;
    }

    private XmlElement CreateLabelSpanElement(string? concept, string? dimension, string? member, string role)
    {
        var span = CreateElement("span");
        span.SetAttribute("class", "xbrl-label");

        // Add data attributes for the JavaScript label resolution
        if (!string.IsNullOrEmpty(concept))
        {
            span.SetAttribute("data-xbrl-concept", concept);
            if (TryGetInitialLabel(concept, role, out var label))
            {
                span.InnerText = label;
            }
        }

        if (!string.IsNullOrEmpty(dimension))
        {
            span.SetAttribute("data-xbrl-dimension", dimension);
            if (TryGetInitialLabel(dimension, role, out var label))
            {
                span.InnerText = label;
            }
        }

        if (!string.IsNullOrEmpty(member))
        {
            span.SetAttribute("data-xbrl-member", member);
            if (TryGetInitialLabel(member, role, out var label))
            {
                span.InnerText = label;
            }
        }

        span.SetAttribute("data-xbrl-role", role);

        // Add language support attributes
        span.SetAttribute("lang", Context.Configuration.DefaultLanguage);
        // span.SetAttribute("data-xbrl-supported-languages", string.Join(",", Context.SupportedLanguages));

        // Add any additional styling classes if specified in configuration
        if (!string.IsNullOrEmpty(Configuration.DefaultConceptLabelStyles))
        {
            var currentClass = span.GetAttribute("class");
            span.SetAttribute("class", $"{currentClass} {Configuration.DefaultConceptLabelStyles}".Trim());
        }

        return span;
    }

    private bool TryGetInitialLabel(string conceptId, string role, out string label)
    {
        label = string.Empty;

        // Try to find the concept in the taxonomy
        if (!Context.Taxonomy.Concepts.TryGetValue(conceptId, out var concept))
        {
            return false;
        }

        if (Context.Configuration.DefaultLanguage == null)
        {
            return false;
        }

        // Get the label based on role and default language
        var labelInfo = GetConceptLabel(concept, role, Context.Configuration.DefaultLanguage);
        if (labelInfo == null)
        {
            // Try fallback to standard label if the requested role is not found
            if (role != "http://www.xbrl.org/2003/role/label")
            {
                labelInfo = GetConceptLabel(concept, "http://www.xbrl.org/2003/role/label", Context.Configuration.DefaultLanguage);
            }
            if (labelInfo == null)
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
            }

        }

        if (labelInfo != null)
        {
            label = labelInfo.Label;
            return true;
        }

        return false;
    }

    private XBRLLabel? GetConceptLabel(XBRLConcept concept, string role, string language)
    {
        return this.Context.Taxonomy.GetConceptLabel(concept.Id, language, role);
    }

}