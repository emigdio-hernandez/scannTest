using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model
{
    /// <summary>
    /// Represents a reference part in XBRL, containing information about a specific aspect of a reference.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class XBRLReferencePart
    {
        /// <summary>
        /// The name of the reference part.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The prefix of the namespace of the reference part.
        /// </summary>
        [JsonProperty("pfx")]
        public string Prefix { get; set; }

        /// <summary>
        /// The value of the reference part.
        /// </summary>
        [JsonProperty("val")]
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLReferencePart"/> class.
        /// </summary>
        public XBRLReferencePart()
        {
            Name = string.Empty;
            Prefix = string.Empty;
            Value = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLReferencePart"/> class.
        /// </summary>
        /// <param name="name">The name of the reference part</param>
        /// <param name="prefix">The prefix of the namespace of the reference part</param>
        /// <param name="value">The value of the reference part</param>
        public XBRLReferencePart(string name, string prefix, string value)
        {
            Name = name;
            Prefix = prefix;
            Value = value;
        }
    }
} 