using System;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Abstract base class for processing template tags in the iXBRL template system.
/// </summary>
public abstract class TagProcessor
{
    protected readonly TemplateConfiguration Configuration;
    protected readonly TemplateContext Context;
    protected readonly XbrlFactFinder FactFinder;

    /// <summary>
    /// Initializes a new instance of the TagProcessor class.
    /// </summary>
    /// <param name="configuration">Template system configuration</param>
    /// <param name="context">Current template processing context</param>
    /// <param name="factFinder">XBRL fact finder instance</param>
    protected TagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        FactFinder = factFinder ?? throw new ArgumentNullException(nameof(factFinder));
    }

    /// <summary>
    /// Processes a template tag and returns the resulting XHTML content.
    /// </summary>
    /// <param name="element">The XML element representing the template tag</param>
    /// <returns>The processed XHTML content</returns>
    public virtual async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        // implementación base si existe
        return new List<XmlNode>();
    }

    #region Protected Helper Methods

    /// <summary>
    /// Evaluates a boolean expression using variables and operators.
    /// </summary>
    /// <param name="condition">The boolean expression to evaluate</param>
    /// <returns>The evaluation result</returns>
    protected bool EvaluateCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition))
            return false;

        // Replace variables with their values
        var evaluatedCondition = Context.EvaluateExpression(condition);

        // Use a scripting engine or expression parser to evaluate the condition
        return Context.EvaluateBoolean(evaluatedCondition);
    }

    /// <summary>
    /// Creates a new XML element in the output document.
    /// </summary>
    /// <param name="name">Element name</param>
    /// <returns>The created element</returns>
    protected XmlElement CreateElement(string name)
    {
        return Context.Document.CreateElement(name);
    }

    /// <summary>
    /// Formats a value using a specified formatter.
    /// </summary>
    /// <param name="value">Value to format</param>
    /// <param name="formatterAlias">Alias of the formatter to use</param>
    /// <returns>The formatted value</returns>
    protected string FormatValue(string value, string? formatterAlias)
    {
        if (string.IsNullOrEmpty(formatterAlias))
            return value;

        if (Configuration.ValueFormatters.TryGetValue(formatterAlias, out var formatter))
            return formatter.Format(value);

        throw new InvalidOperationException($"Formatter '{formatterAlias}' not found");
    }

    /// <summary>
    /// Validates required attributes on an element.
    /// </summary>
    /// <param name="element">Element to validate</param>
    /// <param name="requiredAttributes">List of required attribute names</param>
    protected void ValidateRequiredAttributes(XmlElement element, params string[] requiredAttributes)
    {
        foreach (var attr in requiredAttributes)
        {
            if (!element.HasAttribute(attr))
                throw new InvalidOperationException($"Required attribute '{attr}' missing on {element.Name} element");
        }
    }

    /// <summary>
    /// Gets an attribute value, evaluating any variable references.
    /// </summary>
    /// <param name="element">Element containing the attribute</param>
    /// <param name="attributeName">Name of the attribute</param>
    /// <returns>The evaluated attribute value</returns>
    protected string GetAttributeValue(XmlElement element, string attributeName)
    {
        var value = element.GetAttribute(attributeName);
        return Context.EvaluateExpression(value);
    }

    /// <summary>
    /// Creates an iXBRL fact element.
    /// </summary>
    /// <param name="fact">The XBRL fact</param>
    /// <param name="format">Optional format specification</param>
    /// <returns>The created iXBRL element</returns>
    protected XmlElement CreateIXBRLFactElement(XBRLFact fact, string? format = null)
    {
        var element = CreateElement("ix:nonFraction");

        element.SetAttribute("contextRef", Context.GetContextId(fact));
        if (fact.Unit != null)
            element.SetAttribute("unitRef", fact.Unit.Id);

        element.SetAttribute("name", fact.Concept);

        if (fact.IsNil.HasValue && fact.IsNil.Value)
            element.SetAttribute("xsi:nil", "true");

        if (!string.IsNullOrEmpty(format))
            element.SetAttribute("format", format);

        if (!string.IsNullOrEmpty(fact.Value))
            element.InnerText = fact.Value;

        return element;
    }

    /// <summary>
    /// Validates the current nesting level against configuration limits.
    /// </summary>
    protected void ValidateNestingLevel()
    {
        if (Context.CurrentNestingLevel >= Configuration.MaxNestingLevel)
            throw new InvalidOperationException($"Maximum nesting level of {Configuration.MaxNestingLevel} exceeded");
    }

    /// <summary>
    /// Creates a text node with the specified content.
    /// </summary>
    /// <param name="text">The text content</param>
    /// <returns>The created text node</returns>
    protected XmlText CreateTextNode(string text)
    {
        return Context.Document.CreateTextNode(text);
    }

    /// <summary>
    /// Processes child elements recursively.
    /// </summary>
    /// <param name="element">Parent element whose children should be processed</param>
    /// <returns>List of processed child nodes</returns>
    protected async Task<IEnumerable<XmlNode>> ProcessChildren(XmlElement element)
    {
        Context.CurrentNestingLevel++;
        try
        {
            var results = new List<XmlNode>();
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    var tagProcessor = Context.GetProcessorForElement(childElement);
                    if (tagProcessor != null)
                    {
                        var result = await tagProcessor.Process(childElement);
                        if (result != null)
                            results.AddRange(result);
                    }
                    else
                    {
                        var attributeProcessor = Context.GetProcessorForAttributes(childElement);
                        if (attributeProcessor != null)
                        {
                            var result = await attributeProcessor.Process(childElement);
                            if (result != null)
                                results.AddRange(result);
                        }
                        else
                        {
                            if (child.ChildNodes.Count > 0)
                            {
                                XmlNode nodeToImport = ImportAndProcessAttributesOfNode(child, false);
                                // Recursively process child elements
                                foreach (XmlNode childNode in await ProcessChildren(childElement))
                                {
                                    nodeToImport.AppendChild(childNode);
                                }
                                results.Add(nodeToImport);
                            }
                            else
                            {
                                // Copy non-template elements as-is
                                results.Add(ImportAndProcessAttributesOfNode(child, false));
                            }
                        }
                    }
                }
                else
                {
                    if (child.NodeType == XmlNodeType.Text)
                    {
                        // Process text looking for expressions
                        var text = child.InnerText;
                        if (text.Contains("${"))
                        {
                            // Use the same evaluator we use for attributes
                            text = Context.EvaluateExpression(text);
                        }
                        results.Add(Context.Document.CreateTextNode(text));
                    }
                    else
                    {
                        // Copy text nodes and other node types as-is
                        results.Add(ImportAndProcessAttributesOfNode(child, true));
                    }
                }
            }
            return results;
        }
        finally
        {
            Context.CurrentNestingLevel--;
        }
    }

    private XmlNode ImportAndProcessAttributesOfNode(XmlNode child, bool deep = false)
    {
        var nodeToImport = Context.Document.ImportNode(child, deep);
        if (nodeToImport.Attributes != null)
        {
            foreach (var attr in nodeToImport.Attributes)
            {
                ((XmlAttribute)attr).Value = Context.EvaluateExpression(((XmlAttribute)attr).Value);
            }
        }

        return nodeToImport;
    }

    #endregion
}