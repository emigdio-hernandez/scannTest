using Abax2InlineXBRLGenerator.Util;
using Abax2InlineXBRLGenerator.Model;
using System.Xml;

namespace Abax2InlineXBRLGenerator.Generator;

public abstract class AttributeProcessor : TagProcessor
{
    protected AttributeProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public abstract string AttributeName { get; }

    /// <summary>
    /// Checks if an attribute is a template attribute.
    /// </summary>
    /// <param name="attr">The attribute to check.</param>
    /// <returns>True if the attribute is a template attribute, false otherwise.</returns>
    protected bool IsTemplateAttribute(XmlAttribute attr)
    {
        return attr.NamespaceURI == Configuration.TemplateNamespace;
    }
} 