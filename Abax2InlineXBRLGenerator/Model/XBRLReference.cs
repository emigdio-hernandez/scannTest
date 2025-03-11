using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Abax2InlineXBRLGenerator.Model
{
    /// <summary>
    /// Represents a reference in XBRL, containing information about the documentation or source of a concept.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class XBRLReference
    {
        /// <summary>
        /// The name of the reference.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The list of reference parts that make up this reference.
        /// </summary>
        [JsonProperty("parts")]
        public IList<XBRLReferencePart> ReferenceParts { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLReference"/> class.
        /// </summary>
        public XBRLReference()
        {
            Name = string.Empty;
            ReferenceParts = new List<XBRLReferencePart>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XBRLReference"/> class.
        /// </summary>
        /// <param name="name">The name of the reference</param>
        /// <param name="referenceParts">The list of reference parts</param>
        public XBRLReference(string name, IList<XBRLReferencePart> referenceParts)
        {
            Name = name;
            ReferenceParts = referenceParts ?? new List<XBRLReferencePart>();
        }
    }
} 