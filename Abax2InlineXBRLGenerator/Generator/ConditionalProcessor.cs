using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-if template tag.
/// </summary>
public class ConditionalProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the ConditionalProcessor class.
    /// </summary>
    public ConditionalProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-if tag and returns the content based on the condition evaluation.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "condition");

        var condition = GetAttributeValue(element, "condition");
        var elseId = GetAttributeValue(element, "else-id");
        var results = new List<XmlNode>();
        try
        {

            // Evaluate the condition
            if (EvaluateCondition(condition))
            {
                // Process children of the if block
                foreach (var node in await ProcessChildren(element))
                {
                    results.Add(node);
                }

                // Mark any associated else block to be skipped
                if (!string.IsNullOrEmpty(elseId))
                {
                    Context.MarkElseBlockSkipped(elseId);
                }
            }
            else
            {
                // If condition is false and there's an else block, it will be processed when encountered
                if (!string.IsNullOrEmpty(elseId))
                {
                    Context.RegisterPendingElseBlock(elseId);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            throw new TemplateProcessingException($"Error processing conditional element: {condition}", ex);
        }
    }
}
