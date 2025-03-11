using System;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using AbaxXBRLNetStandard.Constantes;
using AbaxXBRLNetStandard.Taxonomia.Impl;

namespace Abax2InlineXBRLGenerator.Util;

/// <summary>
/// Utility class for building iXBRL elements.
/// </summary>
public class XbrlElementBuilder
{
    private readonly TemplateConfiguration _configuration;
    private readonly Dictionary<string, HashSet<string>> _usedIds;
    private readonly XBRLTaxonomy _taxonomy;

    /// <summary>
    /// Gets the document instance being used by the builder.
    /// </summary>
    public XmlDocument Document { get; private set; }

    /// <summary>
    /// Initializes a new instance of the XbrlElementBuilder class.
    /// </summary>
    public XbrlElementBuilder(XmlDocument document, TemplateConfiguration configuration, XBRLTaxonomy taxonomy)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _usedIds = new Dictionary<string, HashSet<string>>();
        _taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
    }

    /// <summary>
    /// Updates the document instance being used by the builder.
    /// </summary>
    public void UpdateDocument(XmlDocument newDocument)
    {
        Document = newDocument ?? throw new ArgumentNullException(nameof(newDocument));
    }

    /// <summary>
    /// Creates the iXBRL header section with references, resources, and hidden facts.
    /// </summary>
    public XmlElement CreateHeader(XBRLInstanceDocument instance, TemplateConfiguration configuration)
    {
        var header = Document.CreateElement("ix", "header", "http://www.xbrl.org/2013/inlineXBRL");

        // Add hidden section
        // header.AppendChild(CreateHidden());

        // Add references section
        header.AppendChild(CreateReferences(instance));

        // Add resources section (contexts and units)
        header.AppendChild(CreateResources(instance.Contexts.Values, instance.Units.Values, configuration, instance.Taxonomy, instance));

        return header;
    }

    /// <summary>
    /// Creates an iXBRL footnote element.
    /// </summary>
    public XmlElement CreateFootnoteElement(XBRLFootnote footnote)
    {
        var element = Document.CreateElement("ix", "footnote", "http://www.xbrl.org/2013/inlineXBRL");
        element.SetAttribute("id", "footnote_" + footnote.Id);
        element.SetAttribute("xml:lang", footnote.Language);
        return element;
    }

    /// <summary>
    /// Creates an iXBRL fact element based on the provided XBRL fact.
    /// </summary>
    public XmlElement CreateFactElement(XBRLFact fact, string? format = null, string? scale = null, string? sign = null)
    {
        var element = CreateBaseFactElement(fact);

        // Add common attributes

        _taxonomy.Concepts.TryGetValue(fact.Concept, out var taxonomyConcept);
        if (taxonomyConcept != null)
        {
            string prefix = string.Empty;
            foreach (var (key, value) in _taxonomy.Prefixes)
            {
                if (value == taxonomyConcept.Namespace)
                {
                    prefix = key;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                element.SetAttribute("name", $"{prefix}:{taxonomyConcept.Name}");
            }
            else
            {
                element.SetAttribute("name", taxonomyConcept.Name);
            }
        }
        else
        {
            throw new ArgumentException($"Concept {fact.Concept} not found in the taxonomy");
        }

        element.SetAttribute("contextRef", GetContextRef(fact));

        if (fact.Unit != null)
        {
            element.SetAttribute("unitRef", fact.Unit.Id!);
        }

        if (!string.IsNullOrEmpty(format))
        {
            element.SetAttribute("format", format);
        }

        if (!string.IsNullOrEmpty(scale))
        {
            element.SetAttribute("scale", scale);
        }

        if (!string.IsNullOrEmpty(sign))
        {
            element.SetAttribute("sign", sign);
        }

        if (fact.IsNil == true)
        {
            element.SetAttribute("nil", "true", "http://www.w3.org/2001/XMLSchema-instance");
        }

        if(element.LocalName == "nonFraction")
        {
            //if fact.Decimals is not null or empty, set the decimals attribute
            if(!string.IsNullOrEmpty(fact.Decimals))
            {
                element.SetAttribute("decimals", fact.Decimals);
            }else 
            //if fact.Precision is not null or empty, set the precision attribute
            if(!string.IsNullOrEmpty(fact.Precision))
            {
                element.SetAttribute("precision", fact.Precision);
            }else{
                //set decimals attribute to "INF"
                element.SetAttribute("decimals", "INF");
            }

        }

        // Generate and set a unique ID for the factç
        element.SetAttribute("id", fact.Id ?? GenerateUniqueId(_configuration.OutputConfiguration.FactIdFormat));

        return element;
    }

    /// <summary>
    /// Creates a context element with the specified dimensions.
    /// </summary>
    public XmlElement CreateContext(XBRLContext context, TemplateConfiguration configuration, XBRLTaxonomy taxonomy, XBRLInstanceDocument instanceDocument)
    {
        var contextElement = Document.CreateElement("xbrli", "context", "http://www.xbrl.org/2003/instance");
        contextElement.SetAttribute("id", context.Id ?? GenerateUniqueId(configuration.OutputConfiguration.ContextIdFormat));

        // Add entity
        var entityElement = Document.CreateElement("xbrli", "entity", "http://www.xbrl.org/2003/instance");
        var identifierElement = Document.CreateElement("xbrli", "identifier", "http://www.xbrl.org/2003/instance");
        identifierElement.SetAttribute("scheme", configuration.TemplateEntityNamespace);
        identifierElement.InnerText = context.Entity!;
        entityElement.AppendChild(identifierElement);
        contextElement.AppendChild(entityElement);

        // Add period
        var periodElement = CreatePeriodElement(context);
        contextElement.AppendChild(periodElement);

        // Add dimensions if any
        if (context.Dimensions.Any())
        {
            var scenarioElement = Document.CreateElement("xbrli", "scenario", "http://www.xbrl.org/2003/instance");
            foreach (var dimension in context.Dimensions)
            {
                scenarioElement.AppendChild(CreateDimensionElement(dimension.Key, dimension.Value, taxonomy, instanceDocument));
            }
            contextElement.AppendChild(scenarioElement);
        }

        return contextElement;
    }

    /// <summary>
    /// Creates a unit element.
    /// </summary>
    public XmlElement CreateUnit(XBRLUnit unit)
    {
        var unitElement = Document.CreateElement("xbrli", "unit", "http://www.xbrl.org/2003/instance");
        unitElement.SetAttribute("id", unit.Id);

        if (unit.Multipliers != null && unit.Multipliers.Any())
        {
            foreach (var measure in unit.Multipliers)
            {
                var measureElement = Document.CreateElement("xbrli", "measure", "http://www.xbrl.org/2003/instance");
                var unitPrefix = _configuration.NamespaceManager.LookupPrefix(measure.NameSpace);
                measureElement.InnerText = unitPrefix + ":" + measure.Name;
                unitElement.AppendChild(measureElement);
            }
        }
        else if (unit.Numerator != null && unit.Denominator != null && unit.Numerator.Any() && unit.Denominator.Any())
        {
            var divideElement = Document.CreateElement("xbrli", "divide", "http://www.xbrl.org/2003/instance");

            var numeratorElement = Document.CreateElement("xbrli", "unitNumerator", "http://www.xbrl.org/2003/instance");

            foreach (var measure in unit.Numerator)
            {
                var numeratorMeasure = Document.CreateElement("xbrli", "measure", "http://www.xbrl.org/2003/instance");
                var unitPrefix = _configuration.NamespaceManager.LookupPrefix(measure.NameSpace);
                numeratorMeasure.InnerText = unitPrefix + ":" + measure.Name;
                numeratorElement.AppendChild(numeratorMeasure);
            }

            var denominatorElement = Document.CreateElement("xbrli", "unitDenominator", "http://www.xbrl.org/2003/instance");

            foreach (var measure in unit.Denominator)
            {
                var denominatorMeasure = Document.CreateElement("xbrli", "measure", "http://www.xbrl.org/2003/instance");
                var unitPrefix = _configuration.NamespaceManager.LookupPrefix(measure.NameSpace);
                denominatorMeasure.InnerText = unitPrefix + ":" + measure.Name;
                denominatorElement.AppendChild(denominatorMeasure);
            }

            divideElement.AppendChild(numeratorElement);
            divideElement.AppendChild(denominatorElement);
            unitElement.AppendChild(divideElement);
        }

        return unitElement;
    }

    #region Private Methods

    private XmlElement CreateHidden()
    {
        return Document.CreateElement("ix", "hidden", "http://www.xbrl.org/2013/inlineXBRL");
    }

    private XmlElement CreateRelationship(string factId, XBRLFootnote footnote)
    {
        var relationship = Document.CreateElement("ix", "relationship", "http://www.xbrl.org/2013/inlineXBRL");
        relationship.SetAttribute("fromRefs", factId);
        relationship.SetAttribute("toRefs", $"footnote_{footnote.Id}");
        return relationship;
    }

    private XmlElement CreateReferences(XBRLInstanceDocument instance)
    {
        var references = Document.CreateElement("ix", "references", "http://www.xbrl.org/2013/inlineXBRL");

        var schemaRef = Document.CreateElement("link", "schemaRef", "http://www.xbrl.org/2003/linkbase");
        var typeAttr = Document.CreateAttribute("xlink", "type", "http://www.w3.org/1999/xlink");
        typeAttr.Value = "simple";
        schemaRef.Attributes.Append(typeAttr);

        var hrefAttr = Document.CreateAttribute("xlink", "href", "http://www.w3.org/1999/xlink");
        
        //use the configuration provided entry point location of the taxonomy
        if (instance.Taxonomy != null && !String.IsNullOrEmpty(instance.Taxonomy.EntryPointUri))
        {
            hrefAttr.Value = instance.Taxonomy.EntryPointUri;
        }
        else
        {
            hrefAttr.Value = instance.TaxonomyNamespaceUri;
        }

        schemaRef.Attributes.Append(hrefAttr);

        references.AppendChild(schemaRef);
        return references;
    }

    private XmlElement CreateResources(IEnumerable<XBRLContext> contexts, IEnumerable<XBRLUnit> units, TemplateConfiguration configuration, XBRLTaxonomy taxonomy, XBRLInstanceDocument instanceDocument)
    {
        var resources = Document.CreateElement("ix", "resources", "http://www.xbrl.org/2013/inlineXBRL");

        foreach (var context in contexts)
        {
            resources.AppendChild(CreateContext(context, configuration, taxonomy, instanceDocument));
        }

        foreach (var unit in units)
        {
            resources.AppendChild(CreateUnit(unit));
        }

        foreach (var fact in instanceDocument.Facts.Values.Where(x=>x.Footnotes != null && x.Footnotes.Any()))
        {
            if(fact.Footnotes != null)
            {
                foreach (var footnoteId in fact.Footnotes)
                {
                    var footnote = instanceDocument.FootNotes[footnoteId];
                    resources.AppendChild(CreateRelationship(fact.Id, footnote));
                }
            }
        }

        return resources;
    }



    private XmlElement CreateBaseFactElement(XBRLFact fact)
    {
        // Determine the appropriate element type based on the fact's characteristics
        // This is a simplified version; you might want to expand this based on your needs
        var concept = _taxonomy.Concepts[fact.Concept];
        switch (concept.XBRLDataType)
        {
            case Constants.XBRLMonetaryItemType:
            case Constants.XBRLDecimalItemType:
            case Constants.XBRLIntegerItemType:
            case Constants.XBRLNonNegativeIntegerItemType:
            case Constants.XBRLFloatItemType:
            case Constants.XBRLDoubleItemType:
            case Constants.XBRLSharesItemType:
            case Constants.XBRLSharesItemType2003:
            case Constants.XBRLPureItemType:
            case Constants.XBRLPercentItemType:
                return Document.CreateElement("ix", "nonFraction", "http://www.xbrl.org/2013/inlineXBRL");
            case Constants.XBRLBooleanItemType:
            case Constants.XBRLDateTimeItemType:
            case Constants.XBRLTimeItemType:
            case Constants.XBRLDurationItemType:
            case Constants.XBRLGYearItemType:
            case Constants.XBRLGMonthDayItemType:
            case Constants.XBRLGDayItemType:
            case Constants.XBRLGYearMonthItemType:
            case Constants.XBRLNormalizedStringItemType:
            case Constants.XBRLAnyURIItemType:
            case Constants.XBRLQNameItemType:
            case Constants.XBRLXMLItemType:
            case Constants.XBRLStringItemType:
            case Constants.XBRLTokenItemType:
            case Constants.XBRLDateItemType:
            case Constants.XBRLGMonthItemType:
                return Document.CreateElement("ix", "nonNumeric", "http://www.xbrl.org/2013/inlineXBRL");
            case Constants.XBRLFractionItemType:
                return Document.CreateElement("ix", "fraction", "http://www.xbrl.org/2013/inlineXBRL");
            default:
                throw new ArgumentException($"Unsupported fact type: {concept.XBRLDataType}");
        }
    }

    private XmlElement CreatePeriodElement(XBRLContext context)
    {
        var periodElement = Document.CreateElement("xbrli", "period", "http://www.xbrl.org/2003/instance");

        if (context.Period!.PeriodType == Constants.XBRLInstantPeriodType)
        {
            var instant = Document.CreateElement("xbrli", Constants.XBRLInstantPeriodType, "http://www.xbrl.org/2003/instance");
            instant.InnerText = context.Period!.PeriodInstantDate!;
            periodElement.AppendChild(instant);
        }
        else
        {
            var startDate = Document.CreateElement("xbrli", "startDate", "http://www.xbrl.org/2003/instance");
            startDate.InnerText = context.Period!.PeriodStartDate!;
            var endDate = Document.CreateElement("xbrli", "endDate", "http://www.xbrl.org/2003/instance");
            endDate.InnerText = context.Period!.PeriodEndDate!;

            periodElement.AppendChild(startDate);
            periodElement.AppendChild(endDate);
        }

        return periodElement;
    }

    private XmlElement CreateDimensionElement(string dimensionId, string? memberId, XBRLTaxonomy taxonomy, XBRLInstanceDocument instanceDocument)
    {

        if (!taxonomy.Concepts.ContainsKey(dimensionId))
        {
            throw new ArgumentException($"Dimension {dimensionId} not found in the taxonomy");
        }
        var concept = taxonomy.Concepts[dimensionId];
        if (!concept.IsDimensionConcept)
        {
            throw new ArgumentException($"Concept {dimensionId} is not a dimension");
        }
        if (concept.IsTypedDimension)
        {
            var element = Document.CreateElement("xbrldi", "typedMember", "http://xbrl.org/2006/xbrldi");
            string prefix = string.Empty;
            foreach (var (key, value) in _taxonomy.Prefixes)
            {
                if (value == concept.Namespace)
                {
                    prefix = key;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                element.SetAttribute("dimension", $"{prefix}:{concept.Name}");
            }
            else
            {
                element.SetAttribute("dimension", dimensionId);
            }
            if (concept.AdditionalAttributes != null)
            {
                if (concept.AdditionalAttributes.ContainsKey("xbrldt:typedDomainRef"))
                {
                    // create a typed domain element, if the concept has a typed domain ref, it may start with a #, so we need to remove it if it exists. We need to look for the last "_" to get the prefix and the local name. if there is no "_" we just use the whole string as the local name
                    var typedDomainRef = concept.AdditionalAttributes["xbrldt:typedDomainRef"];
                    // if the typed domain ref starts with a #, remove it
                    if (typedDomainRef.StartsWith("#"))
                    {
                        typedDomainRef = typedDomainRef.Substring(1);
                    }
                    var lastUnderscore = typedDomainRef.LastIndexOf('_');
                    var typedPrefix = lastUnderscore == -1 ? string.Empty : typedDomainRef.Substring(0, lastUnderscore);
                    var localName = lastUnderscore == -1 ? typedDomainRef : typedDomainRef.Substring(lastUnderscore + 1);
                    var namespaceUri = taxonomy.Prefixes.ContainsKey(typedPrefix) ? taxonomy.Prefixes[typedPrefix] : string.Empty;
                    var typedDomainElement = Document.CreateElement(typedPrefix, localName, namespaceUri);

                    if (instanceDocument.TypedDimensions.ContainsKey(dimensionId) && instanceDocument.TypedDimensions[dimensionId] != null && instanceDocument.TypedDimensions[dimensionId].Members.Any(m => m.Id == memberId))
                    {
                        var typedMember = instanceDocument.TypedDimensions[dimensionId].Members.First(m => m.Id == memberId);
                        typedDomainElement.InnerText = typedMember.Name;
                        element.AppendChild(typedDomainElement);
                    }
                    else
                    {
                        throw new ArgumentException($"Typed dimension member {memberId} not found in the instance document");
                    }
                }
            }
            return element;
        }
        else
        {
            var element = Document.CreateElement("xbrldi", "explicitMember", "http://xbrl.org/2006/xbrldi");
            string prefix = string.Empty;
            foreach (var (key, value) in _taxonomy.Prefixes)
            {
                if (value == concept.Namespace)
                {
                    prefix = key;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                element.SetAttribute("dimension", $"{prefix}:{concept.Name}");
            }
            else
            {
                element.SetAttribute("dimension", dimensionId);
            }
            if (memberId != null)
            {
                var dimensionMemberConcept = taxonomy.Concepts[memberId];
                string dimensionMemberPrefix = string.Empty;
                foreach (var (key, value) in _taxonomy.Prefixes)
                {
                    if (value == dimensionMemberConcept.Namespace)
                    {
                        dimensionMemberPrefix = key;
                        break;
                    }   
                }
                element.InnerText = dimensionMemberPrefix + ":" + dimensionMemberConcept.Name;
            }
            return element;
        }
    }

    private string GenerateUniqueId(string prefix)
    {
        if (!_usedIds.ContainsKey(prefix))
        {
            _usedIds[prefix] = new HashSet<string>();
        }

        var index = 1;
        string id;
        do
        {
            id = $"{prefix}_{index++}";
        } while (_usedIds[prefix].Contains(id));

        _usedIds[prefix].Add(id);
        return id;
    }

    private string GetContextRef(XBRLFact fact)
    {
        return fact.Context!.Id ?? GenerateUniqueId(_configuration.OutputConfiguration.ContextIdFormat);
    }

    #endregion
}