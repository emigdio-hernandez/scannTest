using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents an XBRL instance document.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLDocumentConfig
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public XBRLDocumentConfig()
    {
        DocumentVariables = new Dictionary<string, string>();
        KeyFacts = new List<XBRLInstanceDocumentKeyFact>();
        Faqs = new List<XBRLInstanceDocumentFAQ>();
    }
    
    /// <summary>
    /// The information type of the report: ex Quarterly Financial Report, Annual Report, etc
    /// </summary>
    public string ReportType { get; set; }

    /// <summary>
    /// The full title of the XBRL instance document ex: ALFA - 1er Trimestre del 2020
    /// </summary>
    public string FullTitle { get; set; }
    /// <summary>
    /// The legal name of the entity ex: ALFA, S.A.B. de C.V.
    /// </summary>
    public string EntityLegalName { get; set; }

    /// <summary>
    /// The name of the entity (ticker) ex: ALFA
    /// </summary>  
    public string EntityName { get; set; }
    /// <summary>
    /// The period of the report ex: 2023-T1
    /// </summary>
    public string ReportPeriod { get; set; }
    /// <summary>
    /// Whether the report is consolidated or not ex: true
    /// </summary>
    public string Consolidated { get; set; }

    /// <summary>
    /// The date of the end of the reporting period ex: 2023-12-31
    /// </summary>
    public string DateOfEndOfReportingPeriod { get; set; }

    /// <summary>
    /// The minimum date of the periods, before this date the entity has not information to disclose ex: 2023-12-31
    /// This date is used to hide the information of the entity before it was founded
    /// </summary>
    public string EntityIncorporationDate {get;set;}

    /// <summary>
    /// The original variables of the XBRL Instance Document
    /// </summary>
    public IDictionary<string, string> DocumentVariables { get; set; }

    /// <summary>
    /// The key facts of the XBRL Instance Document
    /// </summary>
    public IList<XBRLInstanceDocumentKeyFact> KeyFacts { get; set; }

    /// <summary>
    /// The FAQs of the XBRL Instance Document
    /// </summary>
    public IList<XBRLInstanceDocumentFAQ> Faqs { get; set; }


}