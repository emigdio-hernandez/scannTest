using System.Collections;
using System.Diagnostics;
using AbaxXBRLRealTime.Model.JBRL;
using AbaxXBRLRealTime.Shared;
using Aspose.Pdf.Operators;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents an XBRL instance document.
/// </summary>
public class XBRLInstanceDocument
{
    /// <summary>
    /// The ID of the instance document.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The title of the XBRL instance document.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The URI of the taxonomy namespace of the XBRL instance document.
    /// </summary>
    public string TaxonomyNamespaceUri { get; set; }

    /// <summary>
    /// The taxonomy of the XBRL instance document.
    /// </summary>
    public XBRLTaxonomy Taxonomy { get; set; }

    /// <summary>
    /// The entity ID of the entity for which the XBRL instance document is being created.
    /// </summary>
    public string EntityId { get; set; }

    /// <summary>
    /// The XBRL fact dimensions.
    /// </summary>
    public IDictionary<string, XBRLFact> Facts { get; set; }

    /// <summary>
    /// The periods of the XBRL instance document.
    /// </summary>
    public IDictionary<string, XBRLPeriod> Periods { get; set; }
    /// <summary>
    /// The inventory of replaced periods in the XBRL instance document.
    /// In the Key is the alias of the period and in the value is the effective period id
    /// used in the document
    /// </summary>
    public IDictionary<string, string> PeriodAliases { get; set; }
    /// <summary>
    /// The units of the XBRL instance document.
    /// </summary>
    public IDictionary<string, XBRLUnit> Units { get; set; }

    /// <summary>
    /// The contexts of the XBRL instance document.
    /// </summary>
    public IDictionary<string, XBRLContext> Contexts { get; set; }

    /// <summary>
    /// The entities of the XBRL instance document.
    /// </summary>
    public IDictionary<string, XBRLEntity> Entities { get; set; }

    /// <summary>
    /// The footnotes of the XBRL instance document.
    /// </summary>
    public IDictionary<string, XBRLFootnote> FootNotes { get; set; }

    /// <summary>
    /// The XBRL typed dimensions.
    /// </summary>
    public IDictionary<string, XbrlTypedDimension> TypedDimensions { get; set; }
    /// <summary>
    /// The original variables of the XBRL Instance Document
    /// </summary>
    public IDictionary<string, string> DocumentVariables { get; set; }

    
    /// <summary>
    /// The document configuration of the XBRL Instance Document that contains the document title, entity name, 
    /// report period, consolidated, date of end of reporting period, key facts and FAQs And any other document 
    /// configuration data that is relevant to the document.
    /// </summary>
    public XBRLDocumentConfig DocumentConfig { get; set; }

    
    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLInstanceDocumentDTO"/> class.
    /// </summary>
    /// <param name="id">the id of the instance document</param>
    /// <param name="title">the title of the XBRL instance document</param>
    /// <param name="taxonomyNamespaceUri">the URI of the taxonomy of the XBRL instance document</param>
    /// <param name="entityId">the entity ID of the entity for which the XBRL instance document is being created</param>
    public XBRLInstanceDocument(string id, string title, string taxonomyNamespaceUri, string entityId, XBRLTaxonomy taxonomy,IDictionary<string, string> documentVariables)
    {
        Id = id;
        Title = title;
        Taxonomy = taxonomy;
        TaxonomyNamespaceUri = taxonomyNamespaceUri;
        EntityId = entityId;
        TypedDimensions = new Dictionary<string, XbrlTypedDimension>();
        DocumentVariables = documentVariables ?? new Dictionary<string, string>();
        Facts = new Dictionary<string, XBRLFact>();
        Units = new Dictionary<string, XBRLUnit>();
        Periods = new Dictionary<string, XBRLPeriod>();
        Contexts = new Dictionary<string, XBRLContext>();
        Entities = new Dictionary<string, XBRLEntity>();
        FootNotes = new Dictionary<string, XBRLFootnote>();
        PeriodAliases = new Dictionary<string, string>();
        DocumentConfig = new XBRLDocumentConfig();
    }

    /// <summary>
    /// Creates a list of XBRL unit measures.
    /// </summary>
    /// <param name="unitId">the ID of the unit</param>
    /// <param name="queryFacts">the facts to query</param>
    /// <param name="conceptId">the ID of the concept</param>
    /// <returns>the list of XBRL unit measures</returns>
    private static List<XBRLUnitMeasure> CreateMeasureList(string unitId, IList<RealTimeFact> queryFacts, string conceptId)
    {
        var mesuresIdList = queryFacts.Where(x => x.Dimensions[JBRLConstants.ConceptCoreDimensionId] == conceptId && x.Dimensions[JBRLConstants.UnitCoreDimensionId] == unitId).Select(x => x.Value).ToList();
        var measureList = new List<XBRLUnitMeasure>();
        foreach (var measureId in mesuresIdList)
        {
            // Split the measureId into name space and name, but consider that the namespace could be a URI that my contain the separator character
            var separatorIndex = measureId.LastIndexOf(':');
            if (separatorIndex == -1)
            {
                measureList.Add(new XBRLUnitMeasure(measureId));
            }
            else
            {
                measureList.Add(new XBRLUnitMeasure(measureId, measureId.Substring(0, separatorIndex), measureId.Substring(separatorIndex + 1)));
            }
        }

        return measureList;
    }

    /// <summary>
    /// Creates the necessary contexts from the XBRL facts.
    /// </summary>
    /// <returns>a dictionary of unique contexts</returns>
    private IDictionary<string, XBRLContext> CreateContextsFromXBRLFacts()
    {
        var contexts = new Dictionary<string, XBRLContext>();
        foreach (var (fact, index) in Facts.Values.Select((value, index) => (value, index)))
        {
            if (!Entities.ContainsKey(fact.Entity!))
            {
                var entity = Entities.Values.FirstOrDefault(e => e.Identifier == fact.Entity);
                if (entity == null)
                {
                    throw new InvalidOperationException($"Entity {fact.Entity} not found in the instance document.");
                }
                var entityKey = Entities.Keys.FirstOrDefault(k => Entities[k] == entity);
                if (entityKey != null)
                {
                    Entities.Remove(entityKey);
                }
                Entities.Add(entity.Identifier!, entity);
            }
            var context = new XBRLContext
            {
                Entity = fact.Entity,
                Period = fact.Period
            };
            Taxonomy.Concepts.TryGetValue(fact.Concept, out var concept);
            if (concept == null)
            {
                continue;
            }
            if (fact.Dimensions != null && fact.Dimensions.Count > 0)
            {
                foreach (var dimension in fact.Dimensions)
                {
                    context.AddDimension(dimension.DimensionId, dimension.MemberId);
                }
            }
            if (!contexts.ContainsKey(context.Hash))
            {
                context.FactIds.Add(fact.Id);
                context.Id = $"c-{index}";
                contexts.Add(context.Hash, context);
            }
            else
            {
                context = contexts[context.Hash];
                context.FactIds.Add(fact.Id);
            }
            fact.Context = context;
        }
        return contexts;
    }

    /// <summary>
    /// This method will generate a consecutive id for the facts in format f-1, f-2, f-3, etc.
    /// It will also generate a consecutive id for the units in format u-1, u-2, u-3, etc.
    /// It will generate a consecutive id for the contexts in format c-1, c-2, c-3, etc.
    /// </summary>
    public void PrepareFactsContextsAndUnits()
    {
        var factIndex = 0;
        var unitIndex = 0;
        var newUnits = new Dictionary<string, XBRLUnit>();
        foreach (var unit in Units.Values)
        {
            unit.Id = $"u-{unitIndex}";
            unitIndex++;
            newUnits.Add(unit.Id, unit);
        }
        var newFacts = new Dictionary<string, XBRLFact>();
        foreach (var fact in Facts.Values)
        {
            fact.Id = $"f-{factIndex}";
            factIndex++;
            newFacts.Add(fact.Id, fact);
            if (fact.Unit != null)
            {
                if (!newUnits.ContainsKey(fact.Unit.Id!) && !newUnits[fact.Unit.Id!].FactIds.Contains(fact.Id))
                {
                    newUnits[fact.Unit.Id!].FactIds.Add(fact.Id);
                }
            }
        }
        Units = newUnits;
        Facts = newFacts;
        Contexts = CreateContextsFromXBRLFacts();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLInstanceDocument"/> class.
    /// </summary>
    /// <param name="realTimeInstanceDocument">the real-time instance document</param>
    public XBRLInstanceDocument(RealTimeInstanceDocument realTimeInstanceDocument, XBRLTaxonomy taxonomy, IDictionary<string, string> documentVariables)
    {
        Taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
        Id = realTimeInstanceDocument.Id;
        Title = realTimeInstanceDocument.Title;
        TaxonomyNamespaceUri = realTimeInstanceDocument.Taxonomy.EspacioNombresPrincipal;
        EntityId = realTimeInstanceDocument.EntityShortName;
        TypedDimensions = new Dictionary<string, XbrlTypedDimension>();
        Facts = new Dictionary<string, XBRLFact>();
        Units = new Dictionary<string, XBRLUnit>();
        Periods = new Dictionary<string, XBRLPeriod>();
        Entities = new Dictionary<string, XBRLEntity>();
        Contexts = new Dictionary<string, XBRLContext>();
        PeriodAliases = new Dictionary<string, string>();
        FootNotes = new Dictionary<string, XBRLFootnote>();
        DocumentConfig = new XBRLDocumentConfig();
        var footnoteIndex = 1;
        
        DocumentVariables = documentVariables ?? new Dictionary<string, string>();
        foreach (var fact in realTimeInstanceDocument.Facts)
        {
            if (fact.Dimensions[JBRLConstants.ConceptCoreDimensionId].Equals(JBRLConstants.FootNoteContentConceptId))
            {
                if (!FootNotes.ContainsKey(fact.FactId))
                {
                    FootNotes.Add(fact.FactId, new XBRLFootnote(fact, footnoteIndex));
                    footnoteIndex++;
                }
            }
            if (fact.Dimensions[JBRLConstants.ConceptCoreDimensionId].Equals(JBRLConstants.EntityNameConceptId))
            {
                var entityId = fact.Dimensions[JBRLConstants.EntityCoreDimensionId];
                if (!Entities.ContainsKey(entityId))
                {
                    var entitySchemaFact = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.EntitySchemaConceptId && f.Dimensions[JBRLConstants.EntityCoreDimensionId] == entityId);
                    XBRLEntity entity;
                    if (entitySchemaFact != null)
                    {
                        entity = new XBRLEntity(fact.Value, entitySchemaFact.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Entity schema not found for entity {entityId} in the instance document.");
                    }
                    Entities.Add(fact.Dimensions[JBRLConstants.EntityCoreDimensionId], entity);
                }
            }
            if (fact.Dimensions.ContainsKey(JBRLConstants.EntityCoreDimensionId))
            {
                if (!Contexts.ContainsKey(fact.Dimensions[JBRLConstants.EntityCoreDimensionId]))
                {
                    var context = new XBRLContext();
                    context.Entity = fact.Dimensions[JBRLConstants.EntityCoreDimensionId];
                    Contexts.Add(fact.Dimensions[JBRLConstants.EntityCoreDimensionId], context);
                }
            }
            if (fact.Dimensions.ContainsKey(JBRLConstants.UnitCoreDimensionId))
            {
                if (!Units.ContainsKey(fact.Dimensions[JBRLConstants.UnitCoreDimensionId]))
                {
                    var unitId = fact.Dimensions[JBRLConstants.UnitCoreDimensionId];
                    var factTypeOfUnit = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.TypeOfUnitConceptId && f.Dimensions[JBRLConstants.UnitCoreDimensionId] == unitId);
                    var factUnitDescription = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.UnitDescriptionConceptId && f.Dimensions[JBRLConstants.UnitCoreDimensionId] == unitId);
                    var unit = new XBRLUnit(unitId);
                    if (factTypeOfUnit != null)
                    {
                        unit.Type = factTypeOfUnit.Value;
                    }
                    if (factUnitDescription != null)
                    {
                        unit.Description = factUnitDescription.Value;
                    }
                    unit.Multipliers = CreateMeasureList(unitId, realTimeInstanceDocument.Facts, JBRLConstants.MultiplierMeasureConceptId);
                    unit.Numerator = CreateMeasureList(unitId, realTimeInstanceDocument.Facts, JBRLConstants.NumeratorMeasureConceptId);
                    unit.Denominator = CreateMeasureList(unitId, realTimeInstanceDocument.Facts, JBRLConstants.DenominatorMeasureConceptId);
                    Units.Add(unitId, unit);
                }
            }
            if (fact.Dimensions[JBRLConstants.ConceptCoreDimensionId].Equals(JBRLConstants.PeriodTypeConceptId))
            {
                var periodId = fact.Dimensions[JBRLConstants.PeriodCoreDimensionId];
                if (!Periods.ContainsKey(periodId))
                {
                    var period = new XBRLPeriod();
                    period.Id = periodId;
                    period.PeriodType = fact.Value;

                    if (period.PeriodType == JBRLConstants.PeriodTypeInstant)
                    {
                        var factPeriodInstantDate = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.PeriodInstantDateConceptId && f.Dimensions[JBRLConstants.PeriodCoreDimensionId] == periodId);
                        if (factPeriodInstantDate != null)
                        {
                            period.PeriodInstantDate = factPeriodInstantDate.Value;
                        }
                    }
                    else
                    {
                        var factPeriodStartDate = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.PeriodStartDateConceptId && f.Dimensions[JBRLConstants.PeriodCoreDimensionId] == periodId);
                        var factPeriodEndDate = realTimeInstanceDocument.Facts.FirstOrDefault(f => f.Dimensions[JBRLConstants.ConceptCoreDimensionId] == JBRLConstants.PeriodEndDateConceptId && f.Dimensions[JBRLConstants.PeriodCoreDimensionId] == periodId);
                        if (factPeriodStartDate != null)
                        {
                            period.PeriodStartDate = factPeriodStartDate.Value;
                        }
                        if (factPeriodEndDate != null)
                        {
                            period.PeriodEndDate = factPeriodEndDate.Value;
                        }
                    }

                    Periods.Add(fact.Dimensions[JBRLConstants.PeriodCoreDimensionId], period);
                }
            }
            if (fact.Dimensions[JBRLConstants.ConceptCoreDimensionId].Equals(JBRLConstants.TypedMemberConceptId))
            {
                string? typedDimensionId = null;
                foreach (var dimensionId in fact.Dimensions.Keys)
                {
                    if (dimensionId != JBRLConstants.ConceptCoreDimensionId &&
                    dimensionId != JBRLConstants.PeriodCoreDimensionId &&
                    dimensionId != JBRLConstants.UnitCoreDimensionId &&
                    dimensionId != JBRLConstants.EntityCoreDimensionId)
                    {
                        if (realTimeInstanceDocument.Taxonomy.ConceptosPorId.ContainsKey(dimensionId))
                        {
                            var concept = realTimeInstanceDocument.Taxonomy.ConceptosPorId[dimensionId];
                            if (concept.EsDimension && concept.AtributosAdicionales != null)
                            {
                                typedDimensionId = dimensionId;
                                if (!TypedDimensions.ContainsKey(typedDimensionId))
                                {
                                    TypedDimensions.Add(typedDimensionId, new XbrlTypedDimension(typedDimensionId, concept.Nombre));
                                }
                                if (!TypedDimensions[typedDimensionId].Members.Any(m => m.Id == fact.Dimensions[dimensionId]))
                                {
                                    TypedDimensions[typedDimensionId].Members.Add(new XbrlTypedDimensionMember()
                                    {
                                        Id = fact.Dimensions[dimensionId],
                                        Name = fact.Value
                                    });
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        foreach (var fact in realTimeInstanceDocument.Facts)
        {
            if (!JBRLConstants.CoreConceptsIds.Contains(fact.Dimensions[JBRLConstants.ConceptCoreDimensionId]))
            {
                var xbrlFact = new XBRLFact(fact, this);
                if (!Facts.Values.Any(f => f.HashFactId == xbrlFact.HashFactId))
                {
                    Facts.Add(xbrlFact.Id, xbrlFact);
                }
            }
        }
        //inspect the document variables for periods starting with the prefix "periodAlias_" and create the corresponding periods
        foreach (var documentVariable in DocumentVariables)
        {
            if (documentVariable.Key.StartsWith("periodAlias_"))
            {
                var periodId = documentVariable.Key.Substring("periodAlias_".Length);
                var originalPeriodId = documentVariable.Value;
                PeriodAliases[periodId] = originalPeriodId;
                if (Periods.TryGetValue(originalPeriodId,out var aliasPeriod))
                {
                    Periods.Add(periodId, aliasPeriod);
                }
            }
        }

        PrepareFactsContextsAndUnits();
    }
}