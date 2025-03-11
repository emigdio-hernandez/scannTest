
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Abax2InlineXBRLGenerator.Model
{
    /// <summary>
    /// Represents a calculation linkbase in XBRL, containing calculations for concepts within a specific role.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class XBRLCalculationLinkbase
    {
        /// <summary>
        /// Dictionary mapping concept IDs to their summation items.
        /// Key: The concept ID being calculated
        /// Value: List of summation items that contribute to the calculation
        /// </summary>
        [JsonProperty("calcs")]
        public IDictionary<string, IList<XBRLCalculationSummationItem>> Calculations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLCalculationLinkbase"/> class.
        /// </summary>
        public XBRLCalculationLinkbase()
        {
            Calculations = new Dictionary<string, IList<XBRLCalculationSummationItem>>();
        }
    }
} 