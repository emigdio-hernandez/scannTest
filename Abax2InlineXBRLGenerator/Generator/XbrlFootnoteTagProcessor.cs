using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCnbvPersistence.Services.Impl;
using AbaxXBRLNetStandard.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-footnote template tag.
/// </summary>
public class XbrlFootnoteTagProcessor : TagProcessor
{
    public XbrlFootnoteTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        var results = new List<XmlNode>();

        // Obtener el ID de la nota al pie
        var footnoteId = GetAttributeValue(element, "id");
        if (string.IsNullOrEmpty(footnoteId))
        {
            throw new TemplateProcessingException("The footnote id attribute is required");
        }

        // Buscar la nota al pie en el contexto
        if (!Context.InstanceDocument.FootNotes.ContainsKey(footnoteId))
        {
            if (Configuration.StrictValidation)
            {
                throw new TemplateProcessingException($"Footnote with id '{footnoteId}' not found");
            }
            return results;
        }
        var footnote = Context.InstanceDocument.FootNotes[footnoteId];

        // Crear el elemento footnote
        var footnoteElement = Context.ElementBuilder.CreateFootnoteElement(footnote);

        // Procesar el contenido del footnote
        if (!string.IsNullOrEmpty(footnote.BlobValue))
        {
            var qaDomain = "https://qacnbvrssblobs.blob.core.windows.net/";
            var prodDomain = "https://cnbvprodblobs.blob.core.windows.net/";
            var blobValue = footnote.BlobValue.Replace(qaDomain, prodDomain);

            var resultBlob = await AbaxRecBlobStorageService.DownloadBlobFromUir(blobValue);
            if (resultBlob.Success)
            {
                string htmlContent;
                using (var reader = new StreamReader(resultBlob.Result))
                {
                    htmlContent = reader.ReadToEnd();
                }

                try
                {
                    // Decode HTML entities
                    var decodedContent = XmlUtil.ConvertHtmlNamedEntitiesToNumericEntities(htmlContent);

                    var xhtmlDoc = new XmlDocument();
                    xhtmlDoc.PreserveWhitespace = true;

                    xhtmlDoc.LoadXml($"<div xmlns='http://www.w3.org/1999/xhtml'>{decodedContent}</div>");

                    var elementsToRemove = new[] { "meta", "title", "link", "script", "style" };
                     
                    // Remove unwanted elements
                    foreach (var elementName in elementsToRemove)
                    {
                        var nodes = xhtmlDoc.GetElementsByTagName(elementName).Cast<XmlNode>().ToList();
                        foreach (var node in nodes)
                        {
                            node.ParentNode?.RemoveChild(node);
                        }
                    }

                    foreach (XmlNode childNode in xhtmlDoc.DocumentElement.ChildNodes)
                    {
                        var importedNode = footnoteElement.OwnerDocument.ImportNode(childNode, true);
                        footnoteElement.AppendChild(importedNode);
                    }
                }
                catch (XmlException ex)
                {
                    if (Configuration.StrictValidation)
                    {
                        throw new TemplateProcessingException(
                            $"The HTML content of the footnote {footnote.Id} is not valid XHTML: {ex.Message}",
                            ex);
                    }
                    footnoteElement.InnerText = htmlContent;
                }
            }
        }

        // Procesar contenido anidado si existe
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                footnoteElement.AppendChild(node);
            }
        }

        results.Add(footnoteElement);
        return results;
    }
} 