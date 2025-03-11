using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-variable template tag.
/// </summary>
public class VariableTagProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the VariableTagProcessor class.
    /// </summary>
    public VariableTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-variable tag and returns the variable value.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        ValidateRequiredAttributes(element, "name");

        var variableName = GetAttributeValue(element, "name");
        var format = GetAttributeValue(element, "format");
        var defaultValue = GetAttributeValue(element, "default");

        // Get the variable
        var variable = Context.GetVariable(variableName);
        if (variable == null)
        {
            // If variable doesn't exist and default value is provided, use it
            if (!string.IsNullOrEmpty(defaultValue))
            {
                return new List<XmlNode>() { CreateFormattedTextNode(defaultValue, format) };
            }

            if (Configuration.StrictValidation)
            {
                throw new TemplateProcessingException($"Variable '{variableName}' not found and no default value provided");
            }

            return new List<XmlNode>();
        }

        // Get the value
        var value = variable.GetValue()?.ToString() ?? string.Empty;

        // Format the value if a formatter is specified
        return new List<XmlNode>() { CreateFormattedTextNode(value, format) };
    }

    private XmlText CreateFormattedTextNode(string value, string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return CreateTextNode(value);
        }

        try
        {
            // Get the formatter
            if (!Configuration.ValueFormatters.TryGetValue(format, out var formatter))
            {
                throw new TemplateProcessingException($"Formatter '{format}' not found");
            }

            // Apply the formatter
            var formattedValue = formatter.Format(value);
            return CreateTextNode(formattedValue);
        }
        catch (Exception ex) when (ex is not TemplateProcessingException)
        {
            throw new TemplateProcessingException($"Error formatting value '{value}' with formatter '{format}'", ex);
        }
    }
}