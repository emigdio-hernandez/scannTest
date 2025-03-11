using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-if attribute.
/// </summary>
public class ConditionalAttributeProcessor : AttributeProcessor
{
     public override string AttributeName => "xbrl-if";
    /// <summary>
    /// Initializes a new instance of the ConditionalAttributeProcessor class.
    /// </summary>
    public ConditionalAttributeProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }



    /// <summary>
    /// Processes an hh:xbrl-if tag and returns the content based on the condition evaluation.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();

        var condition = GetAttributeValue(element, $"{Configuration.TemplateNamespacePrefix}:{AttributeName}");
      
        var results = new List<XmlNode>();
        try
        {
            // Evaluate the condition
            if (EvaluateCondition(condition))
            {
                // Process the block including the children
                var newElement = Context.Document.CreateElement(
                    element.Prefix,
                    element.LocalName,
                    element.NamespaceURI);
                // Copy the attributes of the original element
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
                results.Add(newElement);
                foreach (var node in await ProcessChildren(element))
                {
                    newElement.AppendChild(node);
                }
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new TemplateProcessingException($"Error processing conditional attribute: {condition}", ex);
        }
    }
} 