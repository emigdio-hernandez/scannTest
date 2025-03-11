using System;
using System.Linq;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLRealTime.Shared;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

/// <summary>
/// Template function that counts facts based on specified search criteria
/// </summary>
public class GetTypedMemberFactsFunction : ITemplateFunction
{
    private readonly XbrlFactFinder _factFinder;
    private readonly TemplateContext _context;
    /// <summary>
    /// Gets the name of the function
    /// </summary>
    public string Name => "getTypedMemberFacts";

    /// <summary>
    /// Initializes a new instance of CountFactsFunction
    /// </summary>
    /// <param name="factFinder">The fact finder instance to use for searching</param>
    public GetTypedMemberFactsFunction(XbrlFactFinder factFinder, TemplateContext context)
    {
        _factFinder = factFinder ?? throw new ArgumentNullException(nameof(factFinder));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Executes the function with the specified arguments
    /// </summary>
    /// <param name="arguments">The arguments to pass to the function</param>
    /// <returns>The number of facts that match the criteria</returns>
    public object Execute(params object[] arguments)
    {
        if (arguments == null || arguments.Length != 1)
        {
            throw new ArgumentException("CountFacts function requires exactly one argument");
        }

        var factFilterJson = arguments[0]?.ToString();

        if (string.IsNullOrEmpty(factFilterJson))
        {
            throw new ArgumentException("Fact filter cannot be null or empty");
        }

     
        try
        {
            // Check if the fact filter is wrapped in quotes
            if (factFilterJson.StartsWith("\"") && factFilterJson.EndsWith("\""))
            {
                factFilterJson = factFilterJson.Substring(1, factFilterJson.Length - 2);
            }
            // Replace all html encoded characters like &lt;, &gt;, &amp;, etc. with actual characters
            factFilterJson = System.Web.HttpUtility.HtmlDecode(factFilterJson);

            var criteria = FactSearchCriteriaParser.Parse(factFilterJson);
            if (criteria == null)
            {
                throw new ArgumentException("Invalid fact filter format");
            }

            var matchingFacts = _factFinder.FindByMultipleCriteria(criteria, _context);

            var typeMemberList = new List<string>();
            foreach (var typedFact in matchingFacts)
            {
                if (typedFact.Dimensions != null)
                {
                    foreach (var dimension in typedFact.Dimensions)
                    {
                        if (dimension.DimensionId == criteria.dimensionWithTypedMember)
                        {
                            if (!typeMemberList.Contains(dimension.MemberId))
                            {
                                typeMemberList.Add(dimension.MemberId);
                            }
                        }
                    }
                }                
            }

            //return typeMemberList;
            return JsonConvert.SerializeObject(typeMemberList);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Failed to parse fact filter", ex);
        }
    }
}
