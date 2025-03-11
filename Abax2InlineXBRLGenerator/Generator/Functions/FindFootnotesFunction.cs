using System;
using System.Linq;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator.Functions;

/// <summary>
/// Template function that finds footnotes based on specified criteria
/// </summary>
public class FindFootnotesFunction : ITemplateFunction
{
    private readonly XbrlFactFinder _factFinder;
    private readonly TemplateContext _context;

    /// <summary>
    /// Gets the name of the function
    /// </summary>
    public string Name => "findFootnotes";

    /// <summary>
    /// Initializes a new instance of FindFootnotesFunction
    /// </summary>
    /// <param name="factFinder">The fact finder instance to use for searching</param>
    /// <param name="context">The template context</param>
    public FindFootnotesFunction(XbrlFactFinder factFinder, TemplateContext context)
    {
        _factFinder = factFinder ?? throw new ArgumentNullException(nameof(factFinder));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Executes the function with the specified arguments
    /// </summary>
    /// <param name="arguments">The arguments to pass to the function</param>
    /// <returns>A list of dictionaries containing footnote information</returns>
    public object Execute(params object[] arguments)
    {
        IEnumerable<string> footnoteIds;

        // Si no hay argumentos, obtener todos los IDs de notas al pie
        if (arguments == null || arguments.Length == 0)
        {
            footnoteIds = _context.InstanceDocument.FootNotes
                .Select(f => f.Key)
                .Where(id => !string.IsNullOrEmpty(id));
        }
        // Si hay un factId, buscar las notas al pie relacionadas con ese hecho
        else
        {
            var factId = arguments[0]?.ToString();
            if (!string.IsNullOrEmpty(factId))
            {
                var fact = _factFinder.FindById(factId);
                if (fact == null)
                {
                    return JsonConvert.SerializeObject(new List<object>());
                }

                footnoteIds = fact.Footnotes
                    .Where(id => !string.IsNullOrEmpty(id));
            }
            // Si hay un factFilter, buscar las notas al pie relacionadas con los hechos que coincidan
            else if (arguments.Length > 1)
            {
                var factFilterJson = arguments[1]?.ToString();
                if (string.IsNullOrEmpty(factFilterJson))
                {
                    return JsonConvert.SerializeObject(new List<object>());
                }

                try
                {
                    // Check if the fact filter is wrapped in quotes
                    if (factFilterJson.StartsWith("\"") && factFilterJson.EndsWith("\""))
                    {
                        factFilterJson = factFilterJson.Substring(1, factFilterJson.Length - 2);
                    }
                    // Replace all html encoded characters
                    factFilterJson = System.Web.HttpUtility.HtmlDecode(factFilterJson);

                    var criteria = FactSearchCriteriaParser.Parse(factFilterJson);
                    if (criteria == null)
                    {
                        throw new ArgumentException("Invalid fact filter format");
                    }

                    var matchingFacts = _factFinder.FindByMultipleCriteria(criteria, _context);
                    
                    footnoteIds = matchingFacts
                        .SelectMany(f => f.Footnotes)
                        .Where(id => !string.IsNullOrEmpty(id))
                        .Distinct();
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException("Failed to parse fact filter", ex);
                }
            }
            else
            {
                return JsonConvert.SerializeObject(new List<object>());
            }
        }

        // Convertir los IDs en diccionarios con la información requerida
        var result = footnoteIds
            .Select(id => _context.InstanceDocument.FootNotes.FirstOrDefault(f => f.Key == id))
            .Select(footnote => new Dictionary<string, object>
            {
                { "Id", footnote.Value.Id },
                { "Index", footnote.Value.Index }
            })
            .ToList();

        return JsonConvert.SerializeObject(result);
    }
}