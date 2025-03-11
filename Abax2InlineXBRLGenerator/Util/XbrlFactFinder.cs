using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Abax2InlineXBRLGenerator.Generator;
using Abax2InlineXBRLGenerator.Model;

namespace Abax2InlineXBRLGenerator.Util;

/// <summary>
/// Provides functionality to search and filter XBRL facts based on various criteria.
/// </summary>
public class XbrlFactFinder
{
    private readonly XBRLInstanceDocument _instanceDocument;
    private readonly IList<XBRLFact> _facts;
    private readonly IDictionary<string, XBRLConcept> _concepts;

    /// <summary>
    /// Initializes a new instance of XbrlFactFinder
    /// </summary>
    /// <param name="instanceDocument">The instance document to search through</param>
    /// <param name="concepts">Dictionary of XBRL concepts for validation</param>
    public XbrlFactFinder(XBRLInstanceDocument instanceDocument, IDictionary<string, XBRLConcept> concepts)
    {
        _instanceDocument = instanceDocument ?? throw new ArgumentNullException(nameof(instanceDocument));
        _facts = _instanceDocument.Facts.Values.ToList();
        _concepts = concepts ?? throw new ArgumentNullException(nameof(concepts));
    }

    /// <summary>
    /// Finds a fact by its unique identifier
    /// </summary>
    /// <param name="factId">The ID of the fact to find</param>
    /// <returns>The found fact or null if not found</returns>
    public XBRLFact? FindById(string factId)
    {
        return _facts.FirstOrDefault(f => f.Id == factId);
    }

    /// <summary>
    /// Finds facts that match all specified criteria
    /// </summary>
    /// <param name="criteria">Search criteria for filtering facts</param>
    /// <param name="context">Template context for evaluating value filters</param>
    /// <returns>Collection of matching facts</returns>
    public IEnumerable<XBRLFact> FindByMultipleCriteria(FactSearchCriteria criteria, TemplateContext? context = null)
    {
        IEnumerable<XBRLFact> query = _facts;

        if (!string.IsNullOrEmpty(criteria.ConceptId))
        {
            query = query.Where(f => f.Concept == criteria.ConceptId);
        }

        if (criteria.Periods?.Length > 0)
        {
            //inspect the criteria periods and replace the periods with alias if the alias is present in the instance document
             var criteriaPeriods = criteria.Periods.Select(period => 
            _instanceDocument.PeriodAliases.TryGetValue(period, out var alias) ? alias : period)
            .ToList();
                
            query = query.Where(f => f.Period != null &&
                                f.Period.Id != null &&
                                criteriaPeriods.Contains(f.Period.Id));
        }

        if (!string.IsNullOrEmpty(criteria.Unit))
        {
            query = query.Where(f => f.Unit != null && f.Unit.Id != null && f.Unit.Id == criteria.Unit);
        }

        if (!string.IsNullOrEmpty(criteria.Entity))
        {
            query = query.Where(f => f.Entity == criteria.Entity);
        }

        if (criteria.Dimensions != null && criteria.Dimensions.Count == 0)
        {
            query = query.Where(f => f.Dimensions == null || f.Dimensions.Count == 0);
        }

        if (criteria.Dimensions?.Count > 0)
        {
            query = query.Where(f => MatchesDimensions(f, criteria.Dimensions));
        }

        if (criteria.ExcludedDimensions?.Count > 0)
        {
            query = query.Where(f => !HasExcludedDimensions(f, criteria.ExcludedDimensions));
        }

        if (criteria.DimensionFilters?.Count > 0)
        {
            query = query.Where(f => MatchesDimensionFilters(f, criteria.DimensionFilters));
        }

        if (!string.IsNullOrEmpty(criteria.ValueFilter) && context != null)
        {
            query = query.Where(f => EvaluateValueFilter(f, criteria.ValueFilter, context));
        }

        return query;
    }

    /// <summary>
    /// Finds a single fact that matches the specified criteria
    /// </summary>
    /// <param name="criteria">Search criteria for filtering facts</param>
    /// <param name="context">Template context for evaluating value filters</param>
    /// <returns>The matching fact or null if not found or multiple matches exist</returns>
    /// <exception cref="InvalidOperationException">Thrown when multiple facts match the criteria</exception>
    public XBRLFact? FindSingleFact(FactSearchCriteria criteria, TemplateContext? context = null)
    {
        var matches = FindByMultipleCriteria(criteria, context).ToList();

        if (matches.Count > 1)
        {
            throw new InvalidOperationException("Multiple facts found matching the specified criteria");
        }

        return matches.FirstOrDefault();
    }

    /// <summary>
    /// Finds a single fact that matches the specified criteria or returns null if not found
    /// </summary>
    public XBRLFact? FindSingleFactOrDefault(FactSearchCriteria criteria, TemplateContext? context = null)
    {
        try
        {
            return FindSingleFact(criteria, context);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds all facts for a specific concept
    /// </summary>
    /// <param name="conceptId">The concept ID to search for</param>
    /// <returns>Collection of facts with the specified concept</returns>
    public IEnumerable<XBRLFact> FindByConceptId(string conceptId)
    {
        return _facts.Where(f => f.Concept == conceptId);
    }

    /// <summary>
    /// Finds facts based on a dimension filter expression
    /// </summary>
    /// <param name="dimensionFilters">Dictionary of dimension filters</param>
    /// <returns>Collection of facts matching the dimension filters</returns>
    public IEnumerable<XBRLFact> FindByDimensionFilters(IDictionary<string, string> dimensionFilters)
    {
        return _facts.Where(f => MatchesDimensionFilters(f, dimensionFilters));
    }

    private bool EvaluateValueFilter(XBRLFact fact, string filter, TemplateContext context)
    {
        filter = filter.Replace("@{", "${");
        var localContext = new TemplateContext(context.Configuration, context.Document, context.Taxonomy, context.InstanceDocument);
        localContext.SetFunctions(context.GetFunctions());
        // Create a dictionary for the fact properties
        var factProperties = new Dictionary<string, string>
        {
            ["value"] = fact.Value ?? string.Empty
            // Podríamos agregar más propiedades del hecho si es necesario
            // ["decimals"] = fact.Decimals?.ToString() ?? string.Empty,
            // ["precision"] = fact.Precision?.ToString() ?? string.Empty,
        };

        var factPropertiesVariable = new TemplateVariable("fact", TemplateVariable.VariableType.Dictionary, TemplateVariable.VariableScope.Local, null);
        factPropertiesVariable.SetValue(factProperties);
        // Add the fact object as a variable
        localContext.SetVariableValue("fact", factPropertiesVariable);
        filter = localContext.EvaluateExpression(filter);
        return localContext.EvaluateBoolean(filter);
    }

    #region Private Methods

    private static bool MatchesDimensions(XBRLFact fact, IDictionary<string, string> dimensions)
    {
        if (fact.Dimensions == null)
        {
            return dimensions.Count == 0;
        }

        foreach (var (dimensionId, memberId) in dimensions)
        {
            var matchingDimension = fact.Dimensions.FirstOrDefault(d => d.DimensionId == dimensionId);
            if (matchingDimension == null || matchingDimension.MemberId != memberId)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesDimensionFilters(XBRLFact fact, IDictionary<string, string> filters)
    {
        if (fact.Dimensions == null)
        {
            return filters.Count == 0;
        }

        foreach (var (dimensionId, filterPattern) in filters)
        {
            var matchingDimension = fact.Dimensions.FirstOrDefault(d => d.DimensionId == dimensionId);
            if (matchingDimension == null)
            {
                return false;
            }

            if (!Regex.IsMatch(matchingDimension.MemberId, filterPattern))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasExcludedDimensions(XBRLFact fact, HashSet<string> excludedDimensions)
    {
        if (fact.Dimensions == null)
        {
            return false;
        }

        return fact.Dimensions.Any(d => excludedDimensions.Contains(d.DimensionId));
    }

    public IEnumerable<XbrlTypedDimensionMember> GetTypedDimensionMembers(string dimensionName)
    {
        return _instanceDocument.TypedDimensions.FirstOrDefault(d => d.Key == dimensionName).Value.Members ?? Enumerable.Empty<XbrlTypedDimensionMember>();
    }

    #endregion
}
