using Abax2InlineXBRLGenerator.Model;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator.Functions
{
    public class GetPresentationLinkbaseFunction : ITemplateFunction
    {
        private readonly XBRLTaxonomy _taxonomy;

        public GetPresentationLinkbaseFunction(XBRLTaxonomy taxonomy)
        {
            _taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
        }

        public string Name => "getPresentationLinkbase";

        public object Execute(params object[] arguments)
        {
            if (arguments.Length < 1 || arguments.Length > 3)
                throw new ArgumentException("Function requires between 1 and 3 arguments");

            if (arguments[0] == null)
                throw new ArgumentException("Role URI cannot be null");

            string roleUri = arguments[0].ToString()!;
            string? startConceptId = arguments.Length > 1 ? arguments[1]?.ToString() : null;
            string? endConceptId = arguments.Length > 2 ? arguments[2]?.ToString() : null;

            if (!_taxonomy.PresentationLinkbases.ContainsKey(roleUri))
                throw new ArgumentException($"Role URI '{roleUri}' not found in taxonomy");

            var items = _taxonomy.PresentationLinkbases[roleUri];
            var filteredItems = FilterPresentationItems(items.PresentationLinkbaseItems, startConceptId, endConceptId);
            
            return "\"" + JsonConvert.SerializeObject(filteredItems, Newtonsoft.Json.Formatting.Indented) + "\"";
        }

        private List<XBRLPresentationLinkbaseItem> FilterPresentationItems(
            IList<XBRLPresentationLinkbaseItem> items, 
            string? startConceptId, 
            string? endConceptId)
        {
            if (string.IsNullOrEmpty(startConceptId) && string.IsNullOrEmpty(endConceptId))
                return items.ToList();

            var result = new List<XBRLPresentationLinkbaseItem>();
            bool startFound = string.IsNullOrEmpty(startConceptId);
            
            foreach (var item in items)
            {
                if (!startFound)
                {
                    if (item.ConceptId == startConceptId)
                    {
                        startFound = true;
                        result.Add(item);
                    }
                    continue;
                }

                if (item.ConceptId == endConceptId)
                    break;

                result.Add(item);
            }

            return result;
        }
    }
} 