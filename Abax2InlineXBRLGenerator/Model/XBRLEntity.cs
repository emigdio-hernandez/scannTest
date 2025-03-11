using System;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents an entity in an XBRL instance document.
/// </summary>
public class XBRLEntity
{
    /// <summary>
    /// The unique identifier of the entity.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="XBRLEntity"/> class.
    /// </summary>
    /// <param name="identifier">the unique identifier of the entity</param>
    /// <param name="schema">the schema of the entity</param>
    public XBRLEntity(string? identifier, string? schema)
    {
        Identifier = identifier;
        Schema = schema;
    }
}
