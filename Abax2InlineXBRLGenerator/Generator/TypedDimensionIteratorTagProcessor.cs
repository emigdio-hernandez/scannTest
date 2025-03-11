using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the xbrl-iterate-typed-dimension tag, which iterates over typed dimension members.
/// </summary>
public class TypedDimensionIteratorTagProcessor : TagProcessor
{
    public TypedDimensionIteratorTagProcessor(
        TemplateConfiguration configuration,
        TemplateContext context,
        XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateRequiredAttributes(element, "name", "member-var");

        var dimensionName = GetAttributeValue(element, "name");
        var memberVar = GetAttributeValue(element, "member-var");
        var indexVar = GetAttributeValue(element, "index-var");
        var filter = element.HasAttribute("filter") ? GetAttributeValue(element, "filter") : null;
        var scope = element.HasAttribute("scope") ? GetAttributeValue(element, "scope") : "local";

        // Get all members of the typed dimension
        var allMembers = FactFinder.GetTypedDimensionMembers(dimensionName);
        var filteredMembers = new List<XbrlTypedDimensionMember>();
        TemplateVariable.VariableScope scopeEnum;

        // Apply filter if it exists
        if (!string.IsNullOrEmpty(filter))
        {
            var regex = new Regex(filter);
            foreach (var member in allMembers)
            {
                if (regex.IsMatch(member.Name))
                {
                    filteredMembers.Add(member);
                }
            }
        }
        else
        {
            filteredMembers.AddRange(allMembers);
        }

        var results = new List<XmlNode>();

        // Create new scope if necessary
        if (scope == "local")
        {
            Context.PushScope();
            scopeEnum = TemplateVariable.VariableScope.Local;
        }
        else
        {
            scopeEnum = TemplateVariable.VariableScope.Global;
        }

        try
        {
            // Iterate over filtered members
            for (int i = 0; i < filteredMembers.Count; i++)
            {
                var memberVariable =  new TemplateVariable(
                    memberVar,
                    TemplateVariable.VariableType.Dictionary,
                    scopeEnum,
                    null
                );
                memberVariable.SetValue(new Dictionary<string, string>
                {
                    { "Name", filteredMembers[i].Name },
                    { "Id", filteredMembers[i].Id }
                });
                // Set current member variable
                Context.SetVariableValue(memberVar, memberVariable);

                // Set index variable if specified
                if (!string.IsNullOrEmpty(indexVar))
                {
                    var indexVariable = new TemplateVariable(
                        indexVar,
                        TemplateVariable.VariableType.Double,
                        scopeEnum,
                        null
                    );
                    indexVariable.SetValue(i);
                    Context.SetVariableValue(indexVar, indexVariable);
                }

                // Process children with the variables set
                results.AddRange(await ProcessChildren(element));
            }
        }
        finally
        {
            // Remove local scope if created
            if (scope == "local")
            {
                Context.PopScope();
            }
        }

        return results;
    }
} 