using System;

namespace Abax2InlineXBRLGenerator.Generator;


/// <summary>
/// Custom exception for template processing errors
/// </summary>
public class TemplateProcessingException : Exception
{
    public TemplateProcessingException(string message) : base(message)
    {
    }

    public TemplateProcessingException(string message, Exception? innerException) 
        : base(message, innerException)
    {
    }
}
