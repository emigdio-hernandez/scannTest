using System.Globalization;
using Abax2InlineXBRLGenerator.Generator.Formatters;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCore.Common.Constants;

namespace Abax2InlineXBRLGenerator.Generator.Impl;
/// <summary>
/// This class is used to render the IFRS 2019 ICS Inline XBRL template.
/// </summary>
public class IFRS2019ICSInlineXBRLRenderer : BaseInlineXBRLRenderer
{

    /// <summary>
    /// Fills the document configuration with the necessary information. using the concepts and data from the instance document.
    /// </summary>
    /// <param name="factFinder">The fact finder to use.</param>
    /// <param name="instanceDocument">The instance document to use.</param>
    /// <returns>The configured TemplateConfiguration object.</returns>
    protected override TemplateConfiguration FillDocumentConfig(XbrlFactFinder factFinder, XBRLInstanceDocument instanceDocument)
    {
        var configuration = new TemplateConfiguration
        {
            TemplateNamespacePrefix = "hh",
            DefaultNumberFormat = "#,##0.00",
            DefaultDateFormat = "yyyy-MM-dd",
            CultureInfo = CultureInfo.InvariantCulture,
            MaxNestingLevel = 100,
            MaxIterations = 1000,
            StrictValidation = true,
            DefaultLanguage = "es"
        };

        //Fill the standard variables of the document
        instanceDocument.DocumentVariables.TryGetValue("entityName", out var entityName);
        var factDateOfEnd = factFinder.FindByConceptId("ifrs-full_DateOfEndOfReportingPeriod2013").FirstOrDefault();
        var factLegalName = factFinder.FindByConceptId("ifrs-full_NameOfReportingEntityOrOtherMeansOfIdentification").FirstOrDefault();
        var factQuarter = factFinder.FindByConceptId("ifrs_mx-cor_20141205_NumeroDeTrimestre").FirstOrDefault();
        var factConsolidated = factFinder.FindByConceptId("ifrs_mx-cor_20141205_Consolidado").FirstOrDefault();
        
        //fetch only the year part of Value of the fact and the quarter part of the fact 
        var periodString = (factDateOfEnd?.Value?.Split('-')?[0] ?? "N/A") + "-" + (factQuarter?.Value ?? "N/A");

        instanceDocument.DocumentConfig.FullTitle = (entityName ?? "N/A") + " - " + periodString;
        instanceDocument.DocumentConfig.EntityLegalName = factLegalName?.Value ?? "N/A";
        instanceDocument.DocumentConfig.EntityName = entityName ?? "N/A";
        instanceDocument.DocumentConfig.ReportPeriod = periodString;
        instanceDocument.DocumentConfig.Consolidated = factConsolidated?.Value ?? "false";
        instanceDocument.DocumentConfig.DateOfEndOfReportingPeriod = factDateOfEnd?.Value ?? "N/A";
        instanceDocument.DocumentConfig.DocumentVariables = instanceDocument.DocumentVariables;
        instanceDocument.DocumentConfig.ReportType = Constants.QuarterlyFinancialReportType;

        //Specific variables used in the template
        var factEquityAndLiabilitiesEndOfTwoYearsAgo = factFinder.FindSingleFact(new FactSearchCriteria()
        {
            ConceptId = "ifrs-full_EquityAndLiabilities",
            Periods = ["ifrs_mx_EndDateOfTwoYearsAgo"]
        });

        instanceDocument.DocumentConfig.DocumentVariables.Add("firstYearIFRS", factEquityAndLiabilitiesEndOfTwoYearsAgo != null ? "true" : "false");

        //Get the minimum Period.PeriodInstantDate or Period.PeriodEndDate date of the periods
        var minimumPeriodDate = instanceDocument.Contexts.Values
            .Where(x => x.Period != null)
            .Min(x => x.Period.PeriodInstantDate != null 
                ? DateTime.Parse(x.Period.PeriodInstantDate) 
                : DateTime.Parse(x.Period.PeriodEndDate));
        
        instanceDocument.DocumentConfig.DocumentVariables.Add("entityIncorporationDate", 
            minimumPeriodDate.ToString(Constants.XBRLPeriodDateFormat));
        instanceDocument.DocumentConfig.EntityIncorporationDate = 
            minimumPeriodDate.ToString(Constants.XBRLPeriodDateFormat);

        //Key facts
        var factRevenue = factFinder.FindSingleFact(new FactSearchCriteria()
        {
            ConceptId = "ifrs-full_Revenue",
            Periods = ["ifrs_mx_AccumulatedOfCurrentYear"]
        });
        if (factRevenue != null)
        {
            instanceDocument.DocumentConfig.KeyFacts.Add(new XBRLInstanceDocumentKeyFact()
            {
                FactId = factRevenue.Id,
                Title = "+230.47% en ingresos",
                Description = "Comparado con el mismo periodo del año anterior, superando las proyecciones más optimistas de los analistas.",
                RoleUri = factRevenue.Roles.FirstOrDefault()
            });
        }

        var factEquity = factFinder.FindSingleFact(new FactSearchCriteria()
        {
            ConceptId = "ifrs-full_Equity",
            Periods = ["ifrs_mx_EndDateOfTheQuarterOfTheCurrentYear"],
            Dimensions = new Dictionary<string, string>()
        });
        if (factEquity != null)
        {
            instanceDocument.DocumentConfig.KeyFacts.Add(new XBRLInstanceDocumentKeyFact()
            {
                FactId = factEquity.Id,
                Title = "-19.3% en capital contable",
                Description = "El capital contable de la empresa al cierre del trimestre tiene una disminución del 19.3% debido a inversiones realizadas y créditos pagados por adelantos.",
                RoleUri = factEquity.Roles.FirstOrDefault()
            });
        }

        var factProfitLoss = factFinder.FindSingleFact(new FactSearchCriteria()
        {
            ConceptId = "ifrs-full_ProfitLoss",
            Periods = ["ifrs_mx_AccumulatedOfCurrentYear"],
            Dimensions = new Dictionary<string, string>()
        });
        if (factProfitLoss != null)
        {
            instanceDocument.DocumentConfig.KeyFacts.Add(new XBRLInstanceDocumentKeyFact()
            {
                FactId = factProfitLoss.Id,
                Title = "+143%  en la Utilidad Neta",
                Description = "Las utilidades de la empresa al cierre del trimestre tienen un incremento del 143% debido a la reducción de costos y la optimización de procesos.",
                RoleUri = factProfitLoss.Roles.FirstOrDefault()
            });
        }

        //FAQs
        var disclosureOfResultsFact = factFinder.FindSingleFact(new FactSearchCriteria()
        {
            ConceptId = "ifrs-mc_DisclosureOfResultsOfOperationsAndProspectsExplanatory",
            Periods = ["ifrs_mx_AccumulatedOfCurrentYear"],
            Dimensions = new Dictionary<string, string>()
        });

        instanceDocument.DocumentConfig.Faqs.Add(new XBRLInstanceDocumentFAQ()
        {
            FactId = disclosureOfResultsFact?.Id,
            RoleUri = disclosureOfResultsFact?.Roles.FirstOrDefault(),
            Question = "¿Qué impacto tuvo la venta de la sede de Jafra México en los resultados del 3T 2024?",
            Answer = "La venta resultó en una pérdida contable no monetaria de 435 millones de pesos, ya que la propiedad se vendió por 385.7 millones, por debajo de su valor en libros de 811 millones. Sin embargo, generará entradas de efectivo después de impuestos de 315 millones durante los próximos tres años."
        });

        instanceDocument.DocumentConfig.Faqs.Add(new XBRLInstanceDocumentFAQ()
        {
            FactId = disclosureOfResultsFact?.Id,
            RoleUri = disclosureOfResultsFact?.Roles.FirstOrDefault(),
            Question = "¿Cómo fue el desempeño de ingresos netos consolidados en el 3T 2024?",
            Answer = "Los ingresos netos consolidados crecieron 6.6% respecto al año anterior, con todas las unidades de negocio logrando crecimiento. Betterware México marcó su cuarto trimestre consecutivo de crecimiento y Jafra México destacó con un aumento del 9.2%."
        });

        instanceDocument.DocumentConfig.Faqs.Add(new XBRLInstanceDocumentFAQ()
        {
            FactId = disclosureOfResultsFact?.Id,
            RoleUri = disclosureOfResultsFact?.Roles.FirstOrDefault(),
            Question = "¿Cuál es la situación del apalancamiento financiero de la empresa?",
            Answer = "El balance se fortaleció con una disminución de la deuda neta a EBITDA del 15.5% hasta 1.76x y un aumento del 38.0% en la cobertura de intereses hasta 3.52x, proporcionando mayor flexibilidad financiera."
        });

        instanceDocument.DocumentConfig.Faqs.Add(new XBRLInstanceDocumentFAQ()
        {
            FactId = disclosureOfResultsFact?.Id,
            RoleUri = disclosureOfResultsFact?.Roles.FirstOrDefault(),
            Question = "¿Qué expectativas tiene BeFra para el cierre de 2024?",
            Answer = "La compañía espera cerrar el año dentro del rango de orientación, con ingresos entre 13,800-14,400 millones de pesos. Sin embargo, el EBITDA estará más cerca del extremo inferior del rango de 2,900-3,100 millones debido al impacto temporal en el margen de Betterware México."
        });

        return configuration;
    }
} 