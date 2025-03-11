using System;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Configuration options for iXBRL output generation
/// </summary>
public class IXBRLOutputConfiguration
{
    /// <summary>
    /// Whether to indent the output XHTML
    /// </summary>
    public bool IndentOutput { get; set; } = true;

    /// <summary>
    /// Whether to include comments in the output explaining template transformations
    /// </summary>
    public bool IncludeDebugComments { get; set; } = false;

    /// <summary>
    /// Whether to escape HTML in textBlockItemType facts
    /// </summary>
    public bool EscapeHtmlInTextBlocks { get; set; } = false;

    /// <summary>
    /// Format to use for fact IDs in the output
    /// </summary>
    public string FactIdFormat { get; set; } = "f-";

    /// <summary>
    /// Format to use for context IDs in the output
    /// </summary>
    public string ContextIdFormat { get; set; } = "c-";

    /// <summary>
    /// Format to use for unit IDs in the output
    /// </summary>
    public string UnitIdFormat { get; set; } = "u-";

    /// <summary>
    /// Whether to include schema references in the output
    /// </summary>
    public bool IncludeSchemaReferences { get; set; } = true;

    /// <summary>
    /// Whether to validate the final document against iXBRL schema
    /// </summary>
    public bool ValidateOutput { get; set; } = true;
}