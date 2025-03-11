

using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCore.Common.Constants;
using AbaxXBRLRealTime.Model.JBRL;
using AbaxXBRLRealTime.Shared;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// The data transfer object for an XBRL fact.
/// </summary>
public class XBRLFact
{
    /// <summary>
    /// The ID of the fact.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// The Id used for the iXBRL report.
    /// </summary>
    [JsonIgnore]
    public string? HashFactId { get; set; }
    /// <summary>
    /// The concept of the fact.
    /// </summary>
    public string Concept { get; set; }
    /// <summary>
    /// The period of the fact.
    /// </summary>
    public XBRLPeriod? Period { get; set; }
    /// <summary>
    /// The context of the fact.
    /// </summary>
    public XBRLContext? Context { get; set; }
    /// <summary>
    /// The unit of the fact.
    /// </summary>
    public XBRLUnit? Unit { get; set; }
    /// <summary>
    /// The entity of the fact.
    /// </summary>
    public string? Entity { get; set; }
    /// <summary>
    /// The contents of the fact.
    /// </summary>
    public string? Contents { get; set; }
    /// <summary>
    /// A flag indicating if the fact is nil.
    /// </summary>
    public bool? IsNil { get; set; }
    /// <summary>
    /// The dimensions of the fact.
    /// </summary>
    public List<XBRLFactDimension>? Dimensions { get; set; }
    /// <summary>
    /// The value of the fact.
    /// </summary>
    public string? Value { get; set; }
    /// <summary>
    /// The roles where the fact is used.
    /// </summary>
    public IList<string> Roles { get; set; }
    /// <summary>
    /// The footnotes of the fact.
    /// </summary>
    public IList<string> Footnotes { get; set; }
    /// <summary>
    /// The rounded value of the fact.
    /// </summary>
    public decimal? RoundedValue { get; set; }
    /// <summary>
    /// The number of decimals of the fact.
    /// </summary>
    public string? Decimals { get; set; }
    /// <summary>
    /// The precision of the fact.
    /// </summary>
    public string? Precision { get; set; }
    /// <summary>
    /// The scale of the fact.
    /// </summary>
    public string? Scale { get; set; }
    /// <summary>
    /// The blob value of the fact.
    /// </summary>
    public string? BlobValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLFactDTO"/> class.
    /// </summary>
    /// <param name="id">the id of the fact</param>
    /// <param name="concept">the concept of the fact</param>
    /// <param name="period">the period of the fact</param>
    /// <param name="entity">the entity of the fact</param>
    /// <param name="value">the value of the fact</param>
    public XBRLFact(string id, string concept, XBRLPeriod period, string entity, string? value = null)
    {
        Id = id;
        Concept = concept;
        Period = period;
        Entity = entity;
        Value = value;
        Roles = new List<string>();
        Footnotes = new List<string>();
    }

    public XBRLFact(RealTimeFact realTimeFact, XBRLInstanceDocument document)
    {
        Id = realTimeFact.FactId;
        Concept = realTimeFact.Dimensions[JBRLConstants.ConceptCoreDimensionId];
        if (realTimeFact.Dimensions.ContainsKey(JBRLConstants.PeriodCoreDimensionId))
        {
            var periodId = realTimeFact.Dimensions[JBRLConstants.PeriodCoreDimensionId];
            document.Periods.TryGetValue(periodId, out var period);
            if (period != null)
            {
                Period = period;
            }
        }

        RoundedValue = realTimeFact.RoundedValue;
        Decimals = realTimeFact.Decimals;
        Precision = realTimeFact.Precision;

        if (realTimeFact.Dimensions.ContainsKey(JBRLConstants.UnitCoreDimensionId))
        {
            var unitId = realTimeFact.Dimensions[JBRLConstants.UnitCoreDimensionId];
            document.Units.TryGetValue(unitId, out var unit);
            if (unit != null)
            {
                Unit = unit;
            }
        }
        if (realTimeFact.Dimensions.ContainsKey(JBRLConstants.EntityCoreDimensionId))
        {
            var entityId = realTimeFact.Dimensions[JBRLConstants.EntityCoreDimensionId];
            document.Entities.TryGetValue(entityId, out var entity);
            if (entity != null)
            {
                Entity = entity.Identifier;
            }
            else
            {
                throw new Exception($"Entity with id {entityId} not found in the document.");
            }
        }
        Footnotes = realTimeFact.FootNotes;
        Roles = realTimeFact.Roles;
        Contents = string.Empty;
        IsNil = realTimeFact.Value == null && realTimeFact.BlobValue == null;
        Dimensions = new List<XBRLFactDimension>();
        foreach (var dimension in realTimeFact.Dimensions)
        {
            if (dimension.Key != JBRLConstants.ConceptCoreDimensionId && dimension.Key != JBRLConstants.PeriodCoreDimensionId && dimension.Key != JBRLConstants.UnitCoreDimensionId && dimension.Key != JBRLConstants.EntityCoreDimensionId)
            {
                Dimensions.Add(new XBRLFactDimension(dimension.Key, dimension.Value));
            }
        }
        Value = realTimeFact.Value;
        BlobValue = realTimeFact.BlobValue;
        HashFactId = Hash(Concept, Unit?.Id, Period?.Id, Entity, Dimensions);
    }

    /// <summary>
    /// Generates a hash string that represents the dimensional information of the fact.
    /// </summary>
    public static string Hash(IDictionary<string, string?> dimensions)
    {
        // check if dimensions is null or if any of the values are null, if any of these conditions are true, replace the value with an empty string, clone the dictionary to avoid modifying the original
        var newDimensions = dimensions?.ToDictionary(x => x.Key, x => x.Value ?? string.Empty) ?? new Dictionary<string, string>();
        var stringDictionary = JsonConvert.SerializeObject(newDimensions);
        return MD5Util.CreateMD5(stringDictionary);
    }

    /// <summary>
    /// Creates a hash string that represents the dimensional information
    /// </summary>
    /// <param name="concept">the concept of the fact</param>
    /// <param name="dimensions">the dimensional information to use to create the hash string</param>
    /// <returns>a hash string that represents the dimensional information</returns>
    public static string Hash(string concept, string? unit, string period, string entity, List<XBRLFactDimension>? dimensionalInformation)
    {
        var sortedDictionary = new SortedDictionary<string, string?>();
        if (!string.IsNullOrEmpty(concept))
        {
            sortedDictionary.Add("concept", concept);
        }
        if (!string.IsNullOrEmpty(unit))
        {
            sortedDictionary.Add("unit", unit);
        }
        if (!string.IsNullOrEmpty(period))
        {
            sortedDictionary.Add("period", period);
        }
        if (!string.IsNullOrEmpty(entity))
        {
            sortedDictionary.Add("entity", entity);
        }
        if (dimensionalInformation != null && dimensionalInformation.Count > 0)
        {
            foreach (var dimension in dimensionalInformation)
            {
                sortedDictionary.Add(dimension.DimensionId, dimension.MemberId);
            }
        }
        return Hash(sortedDictionary);
    }
}
