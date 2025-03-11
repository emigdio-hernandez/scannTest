using System;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCore.Dto.InstanceEditor.Dto;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-iterate-dimension template tag.
/// </summary>
public class DimensionIteratorProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the DimensionIteratorProcessor class.
    /// </summary>
    public DimensionIteratorProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-iterate-dimension tag and returns the processed content.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "dimension", "member-var");

        var dimensionId = GetAttributeValue(element, "dimension");
        var memberVarName = GetAttributeValue(element, "member-var");
        var indexVarName = GetAttributeValue(element, "index-var");
        var filter = GetAttributeValue(element, "filter");
        var scope = GetAttributeValue(element, "scope")?.ToLower() ?? "local";
        var results = new List<XmlNode>();

        // Get dimension members
        var members = GetDimensionMembers(dimensionId, filter);
        if (!members.Any())
        {
            if (Configuration.StrictValidation)
            {
                throw new TemplateProcessingException($"No members found for dimension: {dimensionId}");
            }
            return results;
        }

        // Create a new scope for variables if local scope is specified
        if (scope == "local")
        {
            Context.PushScope();
        }

        try
        {
            // Process each member
            for (var i = 0; i < members.Count; i++)
            {
                // Set member variable
                var memberVar = new TemplateVariable(memberVarName, TemplateVariable.VariableType.String, 
                    scope == "local" ? TemplateVariable.VariableScope.Local : TemplateVariable.VariableScope.Global);
                memberVar.SetValue(members[i].Id);
                Context.SetVariableValue(memberVarName, memberVar);

                // Set index variable if specified
                if (!string.IsNullOrEmpty(indexVarName))
                {
                    var indexVar = new TemplateVariable(indexVarName, TemplateVariable.VariableType.Double,
                        scope == "local" ? TemplateVariable.VariableScope.Local : TemplateVariable.VariableScope.Global);
                    indexVar.SetValue(i.ToString());
                    Context.SetVariableValue(indexVarName, indexVar);
                }

                // Process child elements with the new variables
                foreach (var node in await ProcessChildren(element))
                {
                    results.Add(node);
                }
            }
        }
        finally
        {
            // Remove the variable scope if local
            if (scope == "local")
            {
                Context.PopScope();
            }
        }

        return results;
    }

    /// <summary>
    /// Gets the members of a dimension, optionally filtered by a pattern.
    /// </summary>
    private List<XBRLConcept> GetDimensionMembers(string dimensionId, string? filter)
    {
        // Get the dimension concept
        var dimensionConcept = Context.Taxonomy.Concepts.Values
            .FirstOrDefault(c => c.Id == dimensionId && 
                               c.ItemType == ConceptoDto.DimensionItem);

        if (dimensionConcept == null)
        {
            throw new TemplateProcessingException($"Dimension not found or invalid: {dimensionId}");
        }

        // Get all members for this dimension
        var members = Context.Taxonomy.Concepts.Values
            .Where(c => c.ParentDimension == dimensionId)
            .ToList();

        // Apply filter if specified
        if (!string.IsNullOrEmpty(filter))
        {
            try
            {
                var regex = new Regex(filter, RegexOptions.Compiled);
                members = members.Where(m => regex.IsMatch(m.Id)).ToList();
            }
            catch (RegexParseException ex)
            {
                throw new TemplateProcessingException($"Invalid filter pattern: {filter}", ex);
            }
        }

        return members;
    }
}