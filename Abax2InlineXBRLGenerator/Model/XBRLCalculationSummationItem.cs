using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Abax2InlineXBRLGenerator.Model
{
    /// <summary>
    /// Represents a summation item in an XBRL calculation.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class XBRLCalculationSummationItem
    {
        /// <summary>
        /// The ID of the concept that contributes to the calculation.
        /// </summary>
        [JsonProperty("id")]
        public string ConceptId { get; set; }

        /// <summary>
        /// The weight of this concept in the calculation (typically 1 or -1).
        /// </summary>
        [JsonProperty("w")]
        public decimal Weight { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLCalculationSummationItem"/> class.
        /// </summary>
        /// <param name="conceptId">The ID of the concept</param>
        /// <param name="weight">The weight in the calculation</param>

        public XBRLCalculationSummationItem(string conceptId, decimal weight)
        {
            ConceptId = conceptId;
            Weight = weight;
        }
    }
} 