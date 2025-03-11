using System;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Interface for template functions.
/// </summary>
public interface ITemplateFunction
{
    /// <summary>
    /// Gets the name of the function.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Executes the function with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the function</param>
    /// <returns>The result of the function</returns>
    object Execute(params object[] arguments);
}