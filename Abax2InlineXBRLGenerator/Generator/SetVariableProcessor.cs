using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-set-variable template tag.
/// </summary>
public class SetVariableProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the SetVariableProcessor class.
    /// </summary>
    public SetVariableProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-set-variable tag and sets the variable value.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "name");

        var name = GetAttributeValue(element, "name");
        var scope = GetAttributeValue(element, "scope")?.ToLower();
        var value = GetAttributeValue(element, "value");
        var factId = GetAttributeValue(element, "fact-id");
        var factFilter = GetAttributeValue(element, "fact-filter");

        if (string.IsNullOrEmpty(scope))
        {
            scope = "local";
        }

        // Si no hay valor en los atributos, intentar obtenerlo del contenido
        if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(factId) && string.IsNullOrEmpty(factFilter))
        {
            var elementContent = element.InnerXml?.Trim();
            if (!string.IsNullOrEmpty(elementContent))
            {
                try
                {
                    var evaluatedContent = Context.EvaluateExpression(elementContent);
                    value = evaluatedContent?.ToString()?.Trim();
                }
                catch (Exception ex)
                {
                    throw new TemplateProcessingException($"{element.Name}: Error evaluating element content ({elementContent}) : {ex.Message}", ex);
                }
            }
        }

        // Ahora que tenemos el valor final, determinar el tipo
        var variableType = DetermineVariableType(element, value, factId);

        // Create and set the variable
        var variable = CreateVariable(name, variableType, scope);

        // Set the variable value based on the source
        if (!string.IsNullOrEmpty(factId) || !string.IsNullOrEmpty(factFilter))
        {
            SetVariableFromFact(variable, factId, factFilter);
        }
        else if (!string.IsNullOrEmpty(value))
        {
            SetVariableFromValue(variable, value);
        }

        // Store the variable in the context
        Context.SetVariableValue(name, variable);

        // This tag should not output anything to the document
        return new List<XmlNode>();
    }

    private TemplateVariable.VariableType DetermineVariableType(XmlElement element, string? value, string? factId)
    {
        // Check if type is explicitly specified
        var typeAttr = GetAttributeValue(element, "type")?.ToLower();
        if (!string.IsNullOrEmpty(typeAttr))
        {
            return typeAttr switch
            {
                "string" => TemplateVariable.VariableType.String,
                "double" => TemplateVariable.VariableType.Double,
                "boolean" => TemplateVariable.VariableType.Boolean,
                "dictionary" => TemplateVariable.VariableType.Dictionary,
                "dictionary-array" => TemplateVariable.VariableType.DictionaryArray,
                "string-array" => TemplateVariable.VariableType.StringArray,
                _ => throw new TemplateProcessingException($"{element.Name} Invalid variable type: {typeAttr}")
            };
        }

        // If getting value from fact, determine type from fact's concept
        if (!string.IsNullOrEmpty(factId))
        {
            var fact = FactFinder.FindById(factId);
            if (fact != null)
            {
                return GetVariableTypeFromFactConcept(fact.Concept);
            }
        }

        // Try to infer type from value
        if (!string.IsNullOrEmpty(value))
        {
            if (bool.TryParse(value, out _))
                return TemplateVariable.VariableType.Boolean;

            if (double.TryParse(value, out _))
                return TemplateVariable.VariableType.Double;
            
            try
            {
                if (JsonConvert.DeserializeObject<Dictionary<string, string>>(value) != null)
                    return TemplateVariable.VariableType.Dictionary;
            }
            catch { }

            try
            {
                if (JsonConvert.DeserializeObject<Dictionary<string, string>[]>(value) != null)
                    return TemplateVariable.VariableType.DictionaryArray;
            }
            catch { }

            try
            {
                if (JsonConvert.DeserializeObject<List<string>>(value) != null)
                    return TemplateVariable.VariableType.StringArray;
            }
            catch { }

            return TemplateVariable.VariableType.String;
        }

        // Default to string if type cannot be determined
        return TemplateVariable.VariableType.String;
    }

    private TemplateVariable CreateVariable(string name, TemplateVariable.VariableType type, string scope)
    {
        var variableScope = scope switch
        {
            "global" => TemplateVariable.VariableScope.Global,
            "local" => TemplateVariable.VariableScope.Local,
            _ => throw new TemplateProcessingException($"xbrl-set-variable:Variable name {name}:Invalid scope: {scope}")
        };

        return new TemplateVariable(name, type, variableScope);
    }

    private void SetVariableFromFact(TemplateVariable variable, string? factId, string? factFilter)
    {
        XBRLFact? fact = null;

        if (!string.IsNullOrEmpty(factId))
        {
            fact = FactFinder.FindById(factId);
        }
        else if (!string.IsNullOrEmpty(factFilter))
        {
            try
            {
                var criteria = FactSearchCriteriaParser.Parse(factFilter);
                if (criteria != null)
                {
                    fact = FactFinder.FindSingleFact(criteria, Context);
                }
            }
            catch (JsonException ex)
            {
                throw new TemplateProcessingException($"xbrl-set-variable:Variable name ({variable.Name}) Invalid fact filter JSON for variable: {factFilter} ", ex);
            }
        }

        if (fact == null)
        {
            if (Configuration.StrictValidation)
            {
                throw new TemplateProcessingException($"xbrl-set-variable:Variable name ({variable.Name}) No fact found matching the specified criteria: {(!string.IsNullOrEmpty(factId) ? factId:factFilter)}");
            }
            return;
        }

        variable.SetValue(fact.Value);
    }

    private void SetVariableFromValue(TemplateVariable variable, string value)
    {
        try
        {
            variable.SetValue(value);
        }
        catch (Exception ex)
        {
            throw new TemplateProcessingException($"xbrl-set-variable:Variable name ({variable.Name}) Error setting variable value ({value}) : {ex.Message}", ex);
        }
    }

    private TemplateVariable.VariableType GetVariableTypeFromFactConcept(string conceptId)
    {
        // This would typically look up the concept in your taxonomy
        // and determine the appropriate variable type based on the concept's data type
        // For now, we'll default to String
        return TemplateVariable.VariableType.String;
    }
}
