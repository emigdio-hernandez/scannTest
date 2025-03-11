using System;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Exception thrown when template parsing fails.
/// </summary>
public class TemplateParsingException : Exception
{
    public TemplateParsingException(string message) : base(message)
    {
    }

    public TemplateParsingException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}