using System.Xml;

namespace Abax2InlineXBRLGenerator.Generator;

public interface IAttributeProcessor
{
    Task ProcessAttribute(XmlElement element, string attributeName, string attributeValue);
}