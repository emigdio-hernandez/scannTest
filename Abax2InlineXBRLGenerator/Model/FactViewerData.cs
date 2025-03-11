using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class FactViewerData
{
    [JsonProperty("a")]
    public Dictionary<string, string>? Attributes { get; set; }

    [JsonProperty("v")]
    public string? Value { get; set; }

    [JsonProperty("n")]
    public bool IsNil { get; set; }

    [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
    public string? Decimals { get; set; }

    [JsonProperty("p", NullValueHandling = NullValueHandling.Ignore)]
    public string? Precision { get; set; }

    [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
    public string? Scale { get; set; }

    [JsonProperty("r", NullValueHandling = NullValueHandling.Ignore)]
    public string? RoundedValue { get; set; }

    [JsonProperty("pf", NullValueHandling = NullValueHandling.Ignore)]
    public string? PreviousFact { get; set; }

    [JsonProperty("fv", NullValueHandling = NullValueHandling.Ignore)]
    public string? FormattedValue { get; set; }

    [JsonProperty("f", NullValueHandling = NullValueHandling.Ignore)]
    public string? Format { get; set; }

    [JsonProperty("fn", NullValueHandling = NullValueHandling.Ignore)]
    public IList<IDictionary<string, string>>? Footnotes { get; set; }

    [JsonProperty("rl", NullValueHandling = NullValueHandling.Ignore)]
    public IList<int>? Roles { get; set; }
}