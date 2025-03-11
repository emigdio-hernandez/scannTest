using System.Diagnostics;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCnbvPersistence.Services.Impl;
using Newtonsoft.Json;
using AbaxXBRLNetStandard.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Processor for the hh:xbrl-fact template tag.
/// </summary>
public class XbrlFactTagProcessor : TagProcessor
{
    /// <summary>
    /// Initializes a new instance of the XbrlFactTagProcessor class.
    /// </summary>
    public XbrlFactTagProcessor(TemplateConfiguration configuration, TemplateContext context, XbrlFactFinder factFinder)
        : base(configuration, context, factFinder)
    {
    }

    /// <summary>
    /// Processes an hh:xbrl-fact tag and returns the appropriate iXBRL element.
    /// </summary>
    public override async Task<IEnumerable<XmlNode>> Process(XmlElement element)
    {
        ValidateNestingLevel();
        var results = new List<XmlNode>();
        var format = GetAttributeValue(element, "format");
        format = string.IsNullOrEmpty(format) ? null : format;
        string? formattedValue = null;
        string? decodedBlobValue = null;
        // Verify if the fact is optional
        var isOptional = GetAttributeValue(element, "optional")?.ToLower() == "true";

        // Find the fact based on the provided criteria
        var fact = FindFact(element);
        if (fact == null)
        {
            if (Configuration.StrictValidation && !isOptional)
            {
                throw new TemplateProcessingException($"No fact found matching criteria in element: {element.OuterXml}");
            }
            return results;
        }

        // Determine the appropriate iXBRL tag based on the fact's type
        var factElement = CreateFactElement(fact, element);

        // Process any nested content
        if (element.HasChildNodes)
        {
            foreach (var node in await ProcessChildren(element))
            {
                factElement.AppendChild(node);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(fact.Value) && ((fact.IsNil.HasValue && !fact.IsNil.Value) || fact.IsNil == null))
            {
                try
                {
                    if (!string.IsNullOrEmpty(format) && Configuration.ValueFormatters.TryGetValue(format, out var formatter))
                    {
                        formattedValue = formatter.Format(fact.Value);
                        factElement.InnerText = formattedValue;
                        var xbrlFormatAlias = formatter.XbrlFormatAlias(fact.Value);
                        if (!string.IsNullOrEmpty(xbrlFormatAlias))
                        {
                            factElement.SetAttribute("format", xbrlFormatAlias);
                        }
                        var additionalAttributes = formatter.GetAdditionalAttributes(fact.Value);
                        if (additionalAttributes != null)
                        {
                            foreach (var attr in additionalAttributes)
                            {
                                factElement.SetAttribute(attr.Key, attr.Value);
                            }
                        }
                    }
                    else
                    {
                        factElement.InnerText = fact.Value;
                    }
                }
                catch (Exception ex)
                {
                    if (Configuration.StrictValidation)
                    {
                        throw new TemplateProcessingException("An error occurred while transforming the value of the fact with id: " + fact.Id + " with the iXBRL transformation: " + format, ex);
                    }
                }
            }

            if (string.IsNullOrEmpty(fact.Value) && !string.IsNullOrEmpty(fact.BlobValue))
            {
                var qaDomain = "https://qacnbvrssblobs.blob.core.windows.net/";
                var prodDomain = "https://cnbvprodblobs.blob.core.windows.net/";
                var blobValue = fact.BlobValue.Replace(qaDomain, prodDomain);

                var resultBlob = await AbaxRecBlobStorageService.DownloadBlobFromUir(blobValue);
                if (resultBlob.Success)
                {
                    string htmlContent = string.Empty;
                    using (var reader = new StreamReader(resultBlob.Result))
                    {
                        htmlContent = reader.ReadToEnd();
                    }

                    try
                    {
                        // Decode HTML entities
                        decodedBlobValue = XmlUtil.ConvertHtmlNamedEntitiesToNumericEntities(htmlContent);

                        var settings = new XmlWriterSettings
                        {
                            OmitXmlDeclaration = true,
                            NewLineHandling = NewLineHandling.None,
                            DoNotEscapeUriAttributes = true
                        };

                        var xhtmlDoc = new XmlDocument();
                        xhtmlDoc.PreserveWhitespace = true;

                        xhtmlDoc.LoadXml($"<div xmlns='http://www.w3.org/1999/xhtml'>{decodedBlobValue}</div>");

                        var elementsToRemove = new[] { "meta", "title", "link", "script", "style" };

                        // Remove unwanted elements from the fragment
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
                            var importedNode = factElement.OwnerDocument.ImportNode(childNode, true);
                            factElement.AppendChild(importedNode);
                        }
                    }
                    catch (XmlException ex)
                    {
                        if (Configuration.StrictValidation)
                        {
                            throw new TemplateProcessingException(
                                $"The HTML content of the fact {fact.Id} is not valid XHTML: {ex.Message}",
                                ex);
                        }
                        // If not strict, use the content as plain text
                        if (decodedBlobValue != null)
                        {
                            factElement.InnerText = decodedBlobValue;
                        }
                    }
                }
            }

            if (fact.IsNil.HasValue && fact.IsNil.Value)
            {
                factElement.SetAttribute("nil", "true");
            }

        }

        //This will wrap any negative nonFraction elements in a div with the specified class while maintaining the original XML structure.
        if (factElement.LocalName == "nonFraction")
        {
            var value = fact.Value;
            if (Double.TryParse(value, out var numberValue) && numberValue < 0)
            {
                var wrapperDiv = factElement.OwnerDocument.CreateElement("div","http://www.w3.org/1999/xhtml");
                wrapperDiv.SetAttribute("class", "sign sign-negative");
                wrapperDiv.AppendChild(factElement);
                factElement = wrapperDiv;
            }
        }

        results.Add(factElement);

        // Crear FactViewerData y almacenarlo en el contexto
        var factData = CreateFactViewerData(fact, format, formattedValue, decodedBlobValue);
        Context.AddFactViewerData(fact.Id, factData);

        return results;
    }

    private XBRLFact? FindFact(XmlElement element)
    {
        // Try to find by direct ID first
        var factId = GetAttributeValue(element, "id");
        if (!string.IsNullOrEmpty(factId))
        {
            return FactFinder.FindById(factId);
        }

        var normalCriteria = BuildSearchCriteria(element);
        try
        {
            var fact = FactFinder.FindSingleFact(normalCriteria, Context);
            if (fact == null)
            {
                Debug.WriteLine($"No fact found matching criteria: {JsonConvert.SerializeObject(normalCriteria)}");
            }
            return fact;
        }
        catch (InvalidOperationException ex)
        {
            throw new TemplateProcessingException("Multiple facts match the specified criteria", ex);
        }
    }

    private FactSearchCriteria BuildSearchCriteria(XmlElement element)
    {
        var criteria = new FactSearchCriteria
        {
            ConceptId = GetAttributeValue(element, "concept"),
            Unit = GetAttributeValue(element, "unit"),
            Entity = GetAttributeValue(element, "entity")
        };

        // Process dimensions if present
        var dimensionsAttr = GetAttributeValue(element, "dimensions");
        if (!string.IsNullOrEmpty(dimensionsAttr))
        {
            try
            {
                criteria.Dimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(dimensionsAttr);
            }
            catch (JsonException ex)
            {
                throw new TemplateProcessingException("Invalid dimensions JSON format", ex);
            }
        }

        // Process dimension filters if present
        var filtersAttr = GetAttributeValue(element, "filters");
        if (!string.IsNullOrEmpty(filtersAttr))
        {
            try
            {
                criteria.DimensionFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersAttr);
            }
            catch (JsonException ex)
            {
                throw new TemplateProcessingException("Invalid filters JSON format", ex);
            }
        }

        // Process value filter if present
        var valueFilterAttr = GetAttributeValue(element, "valueFilter");
        if (!string.IsNullOrEmpty(valueFilterAttr))
        {
            criteria.ValueFilter = valueFilterAttr;
        }

        // Process periods if present
        var periodsAttrValue = GetAttributeValue(element, "periods");
        if (!string.IsNullOrEmpty(periodsAttrValue))
        {
            criteria.Periods = JsonConvert.DeserializeObject<string[]>(periodsAttrValue);
        }

        // Process dimensions to exclude if present
        var excludedDimensionsAttr = GetAttributeValue(element, "excludedDimensions");
        if (!string.IsNullOrEmpty(excludedDimensionsAttr))
        {
            try
            {
                criteria.ExcludedDimensions = JsonConvert.DeserializeObject<HashSet<string>>(excludedDimensionsAttr);
            }
            catch (JsonException ex)
            {
                throw new TemplateProcessingException("Invalid excludedDimensions JSON format", ex);
            }
        }

        return criteria;
    }

    private XmlElement CreateFactElement(XBRLFact fact, XmlElement templateElement)
    {
        // Obtener atributos de formato del template
        var format = GetAttributeValue(templateElement, "format");
        var scale = GetAttributeValue(templateElement, "scale");
        var sign = GetAttributeValue(templateElement, "sign");

        // Usar el builder para crear el elemento
        var factElement = Context.ElementBuilder.CreateFactElement(fact, format, scale, sign);

        // Copiar atributos adicionales del template que no son reservados
        foreach (XmlAttribute attr in templateElement.Attributes)
        {
            if (!IsReservedAttribute(attr.LocalName) && !factElement.HasAttribute(attr.LocalName))
            {
                factElement.SetAttribute(attr.LocalName, GetAttributeValue(templateElement, attr.LocalName));
            }
        }

        return factElement;
    }

    private bool IsReservedAttribute(string attributeName)
    {
        return new[] {
            "id",
            "concept",
            "period",
            "optional",
            "periods",
            "unit",
            "entity",
            "dimensions",
            "filters",
            "format",
            "sign",
            "scale",
            "excludedDimensions"
        }.Contains(attributeName);
    }

    private FactViewerData CreateFactViewerData(XBRLFact fact, string? format, string? formattedValue, string? decodedBlobValue)
    {
        var attributes = new Dictionary<string, string>
        {
            { "c", fact.Concept },
            { "e", fact.Entity! },
        };

        if (fact.Unit != null)
        {
            attributes.Add("u", fact.Unit.Id!);
        }

        // Agregar dimensiones
        if (fact.Dimensions != null)
        {
            foreach (var dim in fact.Dimensions)
            {
                if (Context.InstanceDocument.Taxonomy.Concepts.TryGetValue(dim.DimensionId, out var dimConcept))
                {
                    var dimPrefix = Context.Taxonomy.Prefixes.FirstOrDefault(p => p.Value == dimConcept.Namespace).Key;
                    if (dimConcept.IsTypedDimension)
                    {
                        if (Context.InstanceDocument.TypedDimensions.TryGetValue(dim.DimensionId, out var typedDim) && typedDim.Members != null)
                        {
                            var typedMember = typedDim.Members.FirstOrDefault(m => m.Id == dim.MemberId);
                            if (typedMember != null)
                            {
                                attributes.Add($"{dimPrefix}:{dimConcept.Name}", typedMember.Name);
                            }
                        }
                    }
                    else if (Context.InstanceDocument.Taxonomy.Concepts.TryGetValue(dim.MemberId, out var memberConcept))
                    {
                        var memberPrefix = Context.Taxonomy.Prefixes.FirstOrDefault(p => p.Value == memberConcept.Namespace).Key;
                        attributes.Add($"{dimPrefix}:{dimConcept.Name}", $"{memberPrefix}:{memberConcept.Name}");
                    }
                }
            }
        }

        // Agregar periodo
        attributes.Add("p", FormatPeriod(fact.Context!.Period!));
        // Agregar contexto
        attributes.Add("ctx", fact.Context!.Id);
        string? value = fact.Value;
        if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fact.BlobValue) && decodedBlobValue != null)
        {
            value = ExtractBlobContent(decodedBlobValue);
        }

        var previousFact = FindPreviousFact(fact);
        var data = new FactViewerData
        {
            Attributes = attributes,
            Value = value,
            Format = format,
            FormattedValue = formattedValue,
            PreviousFact = previousFact != null ? previousFact.Id : null
        };

        if (fact.Decimals != null)
        {
            data.Decimals = fact.Decimals;
        }

        if (fact.Precision != null)
        {
            data.Precision = fact.Precision;
        }

        if (fact.Scale != null)
        {
            data.Scale = fact.Scale;
        }

        if (fact.RoundedValue != null)
        {
            data.RoundedValue = fact.RoundedValue.ToString();
        }

        if (fact.Roles != null)
        {
            data.Roles = new List<int>();
            foreach (var role in fact.Roles)
            {
                // find index of role in presentation linkbase
                var index = Context.Taxonomy.PresentationLinkbases.Keys.ToList().IndexOf(role);
                if (index != -1)
                {
                    data.Roles.Add(index);
                }
            }
        }

        if (fact.Footnotes != null && fact.Footnotes.Count > 0)
        {
            var footnotes = new List<IDictionary<string, string>>();
            foreach (var footnoteId in fact.Footnotes)
            {
                if (Context.InstanceDocument.FootNotes.TryGetValue(footnoteId, out var footnote))
                {
                    footnotes.Add(new Dictionary<string, string> { { "id", footnoteId }, { "idx", footnote.Index.ToString() } });
                }
            }
            data.Footnotes = footnotes.ToArray();
        }

        return data;
    }


    private string FormatPeriod(XBRLPeriod period)
    {
        if (period.PeriodType == Constants.XBRLInstantPeriodType)
        {
            return period.PeriodInstantDate!;
        }
        return $"{period.PeriodStartDate}/{period.PeriodEndDate}";
    }

    private string ExtractBlobContent(string decodedBlobValue)
    {
        return HtmlTextExtractor.ExtractVisibleText(decodedBlobValue);
    }

    private XBRLFact? FindPreviousFact(XBRLFact currentFact)
    {
        return Context.InstanceDocument.Facts.Values
            .Where(f =>
                f.Concept == currentFact.Concept &&
                f.Entity == currentFact.Entity &&
                f.Unit == currentFact.Unit &&
                HaveSameDimensions(f.Dimensions, currentFact.Dimensions) &&
                IsSamePeriodType(f.Context!.Period, currentFact.Context!.Period) &&
                IsEarlierPeriod(f.Context!.Period, currentFact.Context!.Period))
            .OrderByDescending(f => GetPeriodEndDate(f.Context!.Period))
            .FirstOrDefault();
    }

    private bool HaveSameDimensions(List<XBRLFactDimension>? dims1, List<XBRLFactDimension>? dims2)
    {
        return dims1 != null && dims2 != null && dims1.Count == dims2.Count &&
               !dims1.Except(dims2).Any();
    }

    private bool IsSamePeriodType(XBRLPeriod? p1, XBRLPeriod? p2)
    {
        return p1 != null && p2 != null && p1.PeriodType == p2.PeriodType;
    }

    private bool IsEarlierPeriod(XBRLPeriod? p1, XBRLPeriod? p2)
    {
        if (p1 == null || p2 == null)
            return false;

        var date1 = GetPeriodEndDate(p1);
        var date2 = GetPeriodEndDate(p2);
        return DateTime.Parse(date1) < DateTime.Parse(date2);
    }

    private string GetPeriodEndDate(XBRLPeriod? period)
    {
        if (period == null)
            return string.Empty;

        return period.PeriodType == Constants.XBRLInstantPeriodType ? period.PeriodInstantDate! : period.PeriodEndDate!;
    }
}