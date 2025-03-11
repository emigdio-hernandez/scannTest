using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;

namespace Abax2InlineXBRLGenerator.Util;

/// <summary>
/// Provides validation utilities for the iXBRL template system.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates a variable name according to the template system rules
    /// </summary>
    /// <param name="variableName">Name of the variable to validate</param>
    /// <returns>True if the name is valid, false otherwise</returns>
    public static bool IsValidVariableName(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        // Variable names must start with a letter or underscore
        if (!char.IsLetter(variableName[0]) && variableName[0] != '_')
            return false;

        // Rest of the name can contain letters, numbers, or underscores
        return variableName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Validates if a fact value matches the concept's data type
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="concept">The XBRL concept that defines the expected data type</param>
    /// <returns>True if the value matches the data type, false otherwise</returns>
    public static bool IsValidFactValue(string? value, XBRLConcept concept)
    {
        if (string.IsNullOrEmpty(value))
            return concept.IsNillable;

        return concept.DataType switch
        {
            "xbrli:monetaryItemType" => IsValidMonetary(value),
            "xbrli:stringItemType" => true, // All strings are valid
            "xbrli:booleanItemType" => IsValidBoolean(value),
            "xbrli:dateTimeItemType" => IsValidDateTime(value),
            "xbrli:decimalItemType" => IsValidDecimal(value),
            "xbrli:integerItemType" => IsValidInteger(value),
            "xbrli:percentItemType" => IsValidPercentage(value),
            "xbrli:textBlockItemType" => IsValidTextBlock(value),
            "xbrli:perShareItemType" => IsValidPerShare(value),
            _ => true // Default to valid for unknown types
        };
    }

    /// <summary>
    /// Validates a boolean expression using template variables
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <param name="variables">Dictionary of available variables</param>
    /// <returns>True if the expression is valid, false otherwise</returns>
    public static bool IsValidBooleanExpression(string expression, IDictionary<string, TemplateVariable> variables)
    {
        try
        {
            // Remove all whitespace
            expression = Regex.Replace(expression, @"\s+", "");

            // Validate operators
            var validOperators = new[] { "==", "!=", ">=", "<=", ">", "<", "&&", "||" };
            var containsValidOperator = validOperators.Any(op => expression.Contains(op));
            if (!containsValidOperator)
                return false;

            // Validate variable references
            var variableReferences = Regex.Matches(expression, @"\${([^}]+)}");
            foreach (Match match in variableReferences)
            {
                var variableName = match.Groups[1].Value;
                if (!variables.ContainsKey(variableName))
                    return false;
            }

            // Validate literal values
            var literals = Regex.Matches(expression, @"(?<!\${)[^()&|=<>!]+(?=([)&|=<>]|$))");
            foreach (Match match in literals)
            {
                var literal = match.Value.Trim();
                if (literal.Length > 0 && !IsValidLiteral(literal))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates the structure of dimensions in a fact
    /// </summary>
    /// <param name="fact">The fact to validate</param>
    /// <param name="concepts">Dictionary of available concepts</param>
    /// <returns>True if dimensions are valid, false otherwise</returns>
    public static bool ValidateFactDimensions(XBRLFact fact, IDictionary<string, XBRLConcept> concepts)
    {
        if (fact.Dimensions == null || !fact.Dimensions.Any())
            return true;

        foreach (var dimension in fact.Dimensions)
        {
            // Validate dimension exists
            if (!concepts.TryGetValue(dimension.DimensionId, out var dimensionConcept))
                return false;

            // Validate dimension is actually a dimension
            if (!IsDimensionConcept(dimensionConcept))
                return false;

            // For non-typed dimensions, validate member exists
            if (!dimensionConcept.IsTypedDimension)
            {
                if (!concepts.ContainsKey(dimension.MemberId))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates a context structure
    /// </summary>
    /// <param name="context">The context to validate</param>
    /// <returns>True if the context is valid, false otherwise</returns>
    public static bool ValidateContext(XBRLContext context)
    {
        // Basic required fields
        if (string.IsNullOrEmpty(context.Entity) || 
            context.Period == null ||
            context.Period.PeriodType == null)
            return false;

        // Period validation
        return context.Period.PeriodType switch
        {
            Constants.XBRLInstantPeriodType => !string.IsNullOrEmpty(context.Period.PeriodInstantDate) && 
                        IsValidDateTime(context.Period.PeriodInstantDate),
            
            Constants.XBRLDurationPeriodType => !string.IsNullOrEmpty(context.Period.PeriodStartDate) && 
                         !string.IsNullOrEmpty(context.Period.PeriodEndDate) && 
                         IsValidDateTime(context.Period.PeriodStartDate) && 
                         IsValidDateTime(context.Period.PeriodEndDate) && 
                         DateTime.Parse(context.Period.PeriodStartDate) <= DateTime.Parse(context.Period.PeriodEndDate),
            
            _ => false
        };
    }

    #region Private Helper Methods

    private static bool IsValidMonetary(string value)
    {
        return decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsValidBoolean(string value)
    {
        return bool.TryParse(value, out _) || 
               value == "1" || 
               value == "0";
    }

    private static bool IsValidDateTime(string value)
    {
        return DateTime.TryParse(value, out _);
    }

    private static bool IsValidDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsValidInteger(string value)
    {
        return int.TryParse(value, out _);
    }

    private static bool IsValidPercentage(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var percentage))
        {
            return percentage >= 0 && percentage <= 100;
        }
        return false;
    }

    private static bool IsValidLiteral(string literal)
    {
        return decimal.TryParse(literal, NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
               bool.TryParse(literal, out _) ||
               DateTime.TryParse(literal, out _) ||
               (literal.StartsWith("\"") && literal.EndsWith("\"")) ||
               (literal.StartsWith("'") && literal.EndsWith("'"));
    }

    private static bool IsDimensionConcept(XBRLConcept concept)
    {
        return concept.ItemType != 0;
    }

    /// <summary>
    /// Validates if the value is a valid text block (XHTML fragment or simple text)
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if the value is a valid text block, false otherwise</returns>
    private static bool IsValidTextBlock(string value)
    {
        try
        {
            // Si está vacío o es texto simple, es válido
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('<'))
                return true;

            // Envolver el contenido en un elemento raíz para asegurar un XML válido
            var xmlContent = $"<root>{value}</root>";
            
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                ValidationType = ValidationType.None,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            // Intentar leer el contenido como XML
            while (xmlReader.Read()) { }
            
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if the value is a valid per share amount (decimal number)
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if the value is a valid per share amount, false otherwise</returns>
    private static bool IsValidPerShare(string value)
    {
        // El tipo perShareItemType es un decimal que representa un valor monetario por acción
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal perShareValue))
            return false;

        // Validar que tenga un número razonable de decimales (usualmente no más de 6)
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(perShareValue)[3])[2];
        if (decimalPlaces > 6)
            return false;

        // Los valores por acción no suelen ser extremadamente grandes
        const decimal maxReasonableValue = 1000000m; // Un millón por acción como límite razonable
        return perShareValue > -maxReasonableValue && perShareValue < maxReasonableValue;
    }

    #endregion
}
