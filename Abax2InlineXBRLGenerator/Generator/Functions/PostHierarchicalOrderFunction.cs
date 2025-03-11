using Abax2InlineXBRLGenerator.Model;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator.Functions
{
    public class PostHierarchicalOrderFunction : ITemplateFunction
    {
        public string Name => "postHierarchicalOrder";

        public object Execute(params object[] arguments)
        {
            if (arguments.Length != 1 || arguments[0] == null)
                throw new ArgumentException("Function requires exactly one non-null JSON argument");

            var jsonInput = arguments[0].ToString();
            var items = JsonConvert.DeserializeObject<List<XBRLPresentationLinkbaseItem>>(jsonInput!);
            if (items == null)
                throw new ArgumentException("Invalid JSON input");
            return JsonConvert.SerializeObject(SortHierarchically(items));
        }

        private List<string> SortHierarchically(List<XBRLPresentationLinkbaseItem> items)
        {
            var result = new List<string>();
            var processed = new HashSet<string>();

            for (int i = 0; i < items.Count; i++)
            {
                if (!processed.Contains(items[i].ConceptId!))
                {
                    ProcessNode(items, i, processed, result);
                }
            }

            return result;
        }

        private void ProcessNode(List<XBRLPresentationLinkbaseItem> items, int currentIndex, 
            HashSet<string> processed, List<string> result)
        {
            var currentNode = items[currentIndex];

            if (processed.Contains(currentNode.ConceptId!))
                return;

            if (currentNode.ChildrenCount == 0)
            {
                result.Add(currentNode.ConceptId!);
                processed.Add(currentNode.ConceptId!);
                return;
            }

            var currentLevel = currentNode.Indentation;
            var nextIndex = currentIndex + 1;

            while (nextIndex < items.Count)
            {
                var nextNode = items[nextIndex];

                if (nextNode.Indentation <= currentLevel)
                    break;

                ProcessNode(items, nextIndex, processed, result);
                nextIndex++;
            }

            result.Add(currentNode.ConceptId!);
            processed.Add(currentNode.ConceptId!);
        }
    }
} 