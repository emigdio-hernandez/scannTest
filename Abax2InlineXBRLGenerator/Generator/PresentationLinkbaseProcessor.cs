using System;
using System.Diagnostics;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-presentation-linkbase template tag.
/// </summary>
public class PresentationLinkbaseProcessor : TagProcessor
{
    private readonly XBRLTaxonomy _taxonomy;

    public PresentationLinkbaseProcessor(
        TemplateConfiguration configuration, 
        TemplateContext context, 
        XbrlFactFinder factFinder,
        XBRLTaxonomy taxonomy) 
        : base(configuration, context, factFinder)
    {
        _taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "role", "name");

        var role = GetAttributeValue(element, "role");
        var startFrom = GetAttributeValue(element, "startFrom");
        var stopBefore = GetAttributeValue(element, "stopBefore");
        var variableName = GetAttributeValue(element, "name");
        var results = new List<XmlNode>();
        
        // Verify that the presentation linkbase role exists
        if (!_taxonomy.PresentationLinkbases.TryGetValue(role, out var linkbaseItems))
        {
            throw new TemplateProcessingException($"Presentation linkbase role not found: {role}");
        }

        // Get the items to process
        var itemsToProcess = GetItemsToProcess(linkbaseItems.PresentationLinkbaseItems, startFrom, stopBefore);

        // Convert the items to a dictionary array
        var dictionaryArray = itemsToProcess.Select(item => new Dictionary<string, string>
        {
            ["conceptId"] = item.ConceptId ?? string.Empty,
            ["labelRole"] = item.LabelRole ?? string.Empty,
            ["indentation"] = item.Indentation.ToString(),
            ["isAbstract"] = item.IsAbstract.ToString().ToLower(),
            ["childrenCount"] = item.ChildrenCount.ToString()
        }).ToArray();

        // Create a template variable to store the dictionary array
        var variable = new TemplateVariable(
            variableName,
            TemplateVariable.VariableType.DictionaryArray,
            TemplateVariable.VariableScope.Local);

        variable.SetValue(JsonConvert.SerializeObject(dictionaryArray));

        // Set the variable in the context
        Context.SetVariableValue(variableName, variable);

        // This tag does not render any output
        return results;
    }

    private IList<XBRLPresentationLinkbaseItem> GetItemsToProcess(
        IList<XBRLPresentationLinkbaseItem> allItems, 
        string? startFrom,
        string? stopBefore)
    {
        if (string.IsNullOrEmpty(startFrom))
        {
            return string.IsNullOrEmpty(stopBefore) 
                ? allItems 
                : GetItemsUntilStop(allItems, stopBefore);
        }

        // Find the index of the start concept
        var startIndex = ((List<XBRLPresentationLinkbaseItem>)allItems).FindIndex(item => item.ConceptId == startFrom);
        if (startIndex == -1)
        {
            throw new TemplateProcessingException($"Start concept not found: {startFrom}");
        }

        // Find the index of the stop concept
        var stopIndex = -1;
        if (!string.IsNullOrEmpty(stopBefore))
        {
            stopIndex = ((List<XBRLPresentationLinkbaseItem>)allItems).FindIndex(startIndex, item => item.ConceptId == stopBefore);
            if (stopIndex == -1)
            {
                throw new TemplateProcessingException($"Stop concept not found: {stopBefore}");
            }
        }

        // Find all items between the start and stop concepts
        var itemsToInclude = new List<XBRLPresentationLinkbaseItem>();
        
        for (var i = startIndex; i < allItems.Count; i++)
        {
            var currentItem = allItems[i];
            
            // If the stop concept is found, stop processing
            if (stopIndex != -1 && i == stopIndex)
            {
                break;
            }
            
            itemsToInclude.Add(currentItem);
        }

        return itemsToInclude;
    }

    private IList<XBRLPresentationLinkbaseItem> GetItemsUntilStop(
        IList<XBRLPresentationLinkbaseItem> allItems,
        string stopBefore)
    {
        var stopIndex = ((List<XBRLPresentationLinkbaseItem>)allItems).FindIndex(item => item.ConceptId == stopBefore);
        if (stopIndex == -1)
        {
            throw new TemplateProcessingException($"Stop concept not found: {stopBefore}");
        }

        return allItems.Take(stopIndex).ToList();
    }
}
