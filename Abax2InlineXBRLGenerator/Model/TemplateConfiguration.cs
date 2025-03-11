using System;
using System.Globalization;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator;
using Abax2InlineXBRLGenerator.Generator.Formatters;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Provides configuration options for the iXBRL template processing system.
/// </summary>
public class TemplateConfiguration
{
    /// <summary>
    /// Dictionary of custom value formatters
    /// Key: formatter alias, Value: formatting function
    /// </summary>
    public IDictionary<string, IValueFormatter> ValueFormatters { get; private set; }

    /// <summary>
    /// Dictionary of taxonomy entry point locations
    /// Key: taxonomy namespace, Value: entry point location
    /// </summary>
    //public IDictionary<string, string> TaxonomyEntryPointLocations { get; private set; }

    /// <summary>
    /// Registry of custom template functions
    /// </summary>
    public IDictionary<string, ITemplateFunction> TemplateFunctions { get; private set; }

    /// <summary>
    /// Default concept label styles for inline XBRL output
    /// </summary>
    public string? DefaultConceptLabelStyles { get; set; }

    /// <summary>
    /// Default number format for numeric values when no specific formatter is specified
    /// </summary>
    public string DefaultNumberFormat { get; set; } = "#,##0.00";

    /// <summary>
    /// Default language for labels when no specific language is specified
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Default date format when no specific formatter is specified
    /// </summary>
    public string DefaultDateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Culture info for number and date formatting
    /// </summary>
    public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Maximum allowed nesting level for template tags
    /// </summary>
    public int MaxNestingLevel { get; set; } = 10;

    /// <summary>
    /// Maximum allowed iterations for a single loop construct
    /// </summary>
    public int MaxIterations { get; set; } = 1000;

    /// <summary>
    /// Whether to validate XHTML content in textBlockItemType facts
    /// </summary>
    public bool ValidateXhtmlContent { get; set; } = true;

    /// <summary>
    /// Whether to throw exceptions on validation errors
    /// If false, will log errors but continue processing
    /// </summary>
    public bool StrictValidation { get; set; } = true;

    /// <summary>
    /// Namespace prefix for template tags
    /// </summary>
    public string TemplateNamespacePrefix { get; set; } = "hh";

    /// <summary>
    /// Namespace URI for template tags
    /// </summary>
    public string TemplateNamespace { get; set; } = "http://www.2hsoftware.com.mx/ixbrl/template/hh";

    /// <summary>
    /// Namespace URI for entity tags inside context elements
    /// </summary>
    public string TemplateEntityNamespace { get; set; } = "http://www.sec.gov/CIK";

    /// <summary>
    /// Dictionary of default variables available in all templates
    /// </summary>
    public IDictionary<string, TemplateVariable> GlobalVariables { get; private set; }

    /// <summary>
    /// Configuration for the iXBRL output
    /// </summary>
    public IXBRLOutputConfiguration OutputConfiguration { get; set; }

    /// <summary>
    /// XML namespace manager for template processing
    /// </summary>
    public XmlNamespaceManager NamespaceManager;

    public TemplateConfiguration()
    {
        ValueFormatters = new Dictionary<string, IValueFormatter>();
        GlobalVariables = new Dictionary<string, TemplateVariable>();
        OutputConfiguration = new IXBRLOutputConfiguration();
        InitializeDefaultFormatters();
        //TaxonomyEntryPointLocations = new Dictionary<string, string>();
        TemplateFunctions = new Dictionary<string, ITemplateFunction>();
        //InitializeDefaultTaxonomyEntryPointLocations();
        NamespaceManager = new XmlNamespaceManager(new NameTable());
    }

    /// <summary>
    /// Initializes the default taxonomy entry point locations
    /// </summary>
    /*
    private void InitializeDefaultTaxonomyEntryPointLocations()
    {
        TaxonomyEntryPointLocations["http://www.cnbv.gob.mx/taxonomy/ifrs_mx/full_ifrs_mc_mx_ics_entry_point_2019-01-01"] = "https://taxonomiasxbrl.cnbv.gob.mx/taxonomy/mx-ifrs-2019-01-01/full_ifrs_mc_mx_ics_entry_point_2019-01-01.xsd";
    }
    */
    /// <summary>
    /// Registers a custom value formatter
    /// </summary>
    /// <param name="alias">Alias to identify the formatter</param>
    /// <param name="formatter">Formatting function</param>
    public void RegisterFormatter(string alias, IValueFormatter formatter)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException("Formatter alias cannot be empty", nameof(alias));

        ValueFormatters[alias] = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    /// <summary>
    /// Adds or updates a global variable
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="variable">Template variable</param>
    public void SetGlobalVariable(string name, TemplateVariable variable)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));

        GlobalVariables[name] = variable ?? throw new ArgumentNullException(nameof(variable));
    }

    private void InitializeDefaultFormatters()
    {
        RegisterFormatter("ixt:num-dot-decimal", new NumDotDecimalFormatter());
        RegisterFormatter("ixt:date-day-month-year", new DateFormatter("dd/MM/yyyy"));
        RegisterFormatter("hh:yes-no-boolean", new YesNoBooleanFormatter());
    }
}