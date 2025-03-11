using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-else template tag.
/// </summary>
public class ElseProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the ElseProcessor class.
    /// </summary>
    public ElseProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-else tag and returns the content if the associated if condition was false.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "id");

        var id = GetAttributeValue(element, "id");
        var results = new List<XmlNode>();

        // Check if this else block should be processed
        if (Context.ShouldProcessElseBlock(id))
        {
            // Process children of the else block
            foreach (var node in await ProcessChildren(element))
            {
                results.Add(node);
            }
        }

        // Clear the else block status
        Context.ClearElseBlockStatus(id);

        return results;
    }
}