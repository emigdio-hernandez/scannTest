using System;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-iterate-variable template tag.
/// </summary>
public class VariableIteratorProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the VariableIteratorProcessor class.
    /// </summary>
    public VariableIteratorProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-iterate-variable tag and returns the processed content.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "variable", "item-var");

        var variableName = GetAttributeValue(element, "variable");
        var itemVarName = GetAttributeValue(element, "item-var");
        var indexVarName = GetAttributeValue(element, "index-var");
        var filter = GetAttributeValue(element, "filter");
        var scope = GetAttributeValue(element, "scope")?.ToLower() ?? "local";
        var results = new List<XmlNode>();

        // Get the array variable
        var arrayVariable = Context.GetVariable(variableName);
        if (arrayVariable == null ||
            (arrayVariable.Type != TemplateVariable.VariableType.DictionaryArray &&
             arrayVariable.Type != TemplateVariable.VariableType.StringArray))
        {
            if (Configuration.StrictValidation)
            {
                throw new TemplateProcessingException(
                    $"Variable '{variableName}' not found or is not an array type");
            }
            return results;
        }

        // Process based on array type
        if (arrayVariable.Type == TemplateVariable.VariableType.DictionaryArray)
        {
            return await ProcessDictionaryArray(element, arrayVariable, itemVarName, indexVarName, filter, scope);
        }
        else // StringArray
        {
            return await ProcessStringArray(element, arrayVariable, itemVarName, indexVarName, filter, scope);
        }
    }

    private async Task<IEnumerable<XmlNode>> ProcessDictionaryArray(
        XmlElement element,
        TemplateVariable arrayVariable,
        string itemVarName,
        string? indexVarName,
        string? filter,
        string scope)
    {
        var results = new List<XmlNode>();
        var array = arrayVariable.GetValue<Dictionary<string, string>[]>();
        if (array == null || array.Length == 0)
            return results;

        var filteredArray = FilterDictionaryArray(array, filter);

        if (scope == "local")
            Context.PushScope();

        try
        {
            for (var i = 0; i < filteredArray.Length; i++)
            {
                await ProcessArrayItem(
                    element,
                    filteredArray[i],
                    itemVarName,
                    indexVarName,
                    i,
                    scope,
                    TemplateVariable.VariableType.Dictionary,
                    results);
            }
        }
        finally
        {
            if (scope == "local")
                Context.PopScope();
        }

        return results;
    }

    private async Task<IEnumerable<XmlNode>> ProcessStringArray(
        XmlElement element,
        TemplateVariable arrayVariable,
        string itemVarName,
        string? indexVarName,
        string? filter,
        string scope)
    {
        var results = new List<XmlNode>();
        var array = arrayVariable.GetValue<string[]>();
        if (array == null || array.Length == 0)
            return results;

        var filteredArray = FilterStringArray(array, filter);

        if (scope == "local")
            Context.PushScope();

        try
        {
            for (var i = 0; i < filteredArray.Length; i++)
            {
                await ProcessArrayItem(
                    element,
                    filteredArray[i],
                    itemVarName,
                    indexVarName,
                    i,
                    scope,
                    TemplateVariable.VariableType.String,
                    results);
            }
        }
        finally
        {
            if (scope == "local")
                Context.PopScope();
        }

        return results;
    }

    private async Task ProcessArrayItem(
        XmlElement element,
        object itemValue,
        string itemVarName,
        string? indexVarName,
        int index,
        string scope,
        TemplateVariable.VariableType itemType,
        List<XmlNode> results)
    {
        // Set item variable
        var itemVar = new TemplateVariable(
            itemVarName,
            itemType,
            scope == "local" ? TemplateVariable.VariableScope.Local : TemplateVariable.VariableScope.Global);

        itemVar.SetValue(itemType == TemplateVariable.VariableType.Dictionary
            ? JsonConvert.SerializeObject(itemValue)
            : itemValue.ToString());

        Context.SetVariableValue(itemVarName, itemVar);

        // Set index variable if specified
        if (!string.IsNullOrEmpty(indexVarName))
        {
            var indexVar = new TemplateVariable(
                indexVarName,
                TemplateVariable.VariableType.Double,
                scope == "local" ? TemplateVariable.VariableScope.Local : TemplateVariable.VariableScope.Global);

            indexVar.SetValue(index.ToString());
            Context.SetVariableValue(indexVarName, indexVar);
        }

        // Process children
        foreach (var node in await ProcessChildren(element))
        {
            results.Add(node);
        }
    }

    private Dictionary<string, string>[] FilterDictionaryArray(
        Dictionary<string, string>[] array,
        string? filter)
    {
        if (string.IsNullOrEmpty(filter))
            return array;

        try
        {
            var regex = new Regex(filter, RegexOptions.Compiled);
            return array.Where(dict =>
                dict.Any(kvp =>
                    regex.IsMatch(kvp.Key) || regex.IsMatch(kvp.Value)
                )
            ).ToArray();
        }
        catch (RegexParseException ex)
        {
            throw new TemplateProcessingException($"Invalid filter pattern: {filter}", ex);
        }
    }

    private string[] FilterStringArray(string[] array, string? filter)
    {
        if (string.IsNullOrEmpty(filter))
            return array;

        try
        {
            var regex = new Regex(filter, RegexOptions.Compiled);
            return array.Where(item => regex.IsMatch(item)).ToArray();
        }
        catch (RegexParseException ex)
        {
            throw new TemplateProcessingException($"Invalid filter pattern: {filter}", ex);
        }
    }

}
