using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator;
using Abax2InlineXBRLGenerator.Generator.Formatters;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLRealTime.Shared;

namespace TestAbax2InlineXBRLGenerator;

public class IXBRLDimensionIteratorTests
{
    private TemplateProcessor _processor;
    private XBRLInstanceDocument _instanceDocument;
    private XBRLTaxonomy _taxonomy;
    private TemplateConfiguration _configuration;
    [SetUp]
    public void Setup()
    {
        _taxonomy = new XBRLTaxonomy(
            "ifrs-full",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "IFRS Full",
            "2024-01-01");

        _taxonomy.Concepts["ifrs-full:CashAndCashEquivalents"] = new XBRLConcept(
            "ifrs-full:CashAndCashEquivalents",
            "CashAndCashEquivalents",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            Constants.XBRLInstantPeriodType,
            "credit",
            "xbrli:monetaryItemType",
            "xbrli:monetaryItemType",
            1,
            false,
            false,
            true);

        _taxonomy.Concepts["ifrs-full:Revenue"] = new XBRLConcept(
            "ifrs-full:Revenue",
            "Revenue",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            Constants.XBRLInstantPeriodType,
            "credit",
            "xbrli:monetaryItemType",
            "xbrli:monetaryItemType",
            1,
            false,
            false,
            true);

        _taxonomy.Concepts["dim:ProductType"] = new XBRLConcept(
            "dim:ProductType",
            "ProductType",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            Constants.XBRLDurationPeriodType,
            null,
            "xbrli:stringItemType",
            "xbrldt:dimensionItem",
            4,
            false, false, true, true, false
        );

        // Agregar miembros de la dimensión
        var productMembers = new[] { "Software", "Consulting", "Support", "Training" };
        foreach (var member in productMembers)
        {
            _taxonomy.Concepts[$"mem:{member}"] = new XBRLConcept(
                $"mem:{member}",
                member,
                "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
                Constants.XBRLDurationPeriodType,
                null,
                "xbrli:stringItemType",
                "xbrli:item",
                2,
                false,
                false, true, false, true,
                "dim:ProductType");
        }

        // Configurar el documento instancia
        _instanceDocument = new XBRLInstanceDocument(
            "test-instance",
            "Test Instance",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "2HSoftware", _taxonomy,null);

        var entity = new XBRLEntity("2HSoftware", "http://www.2hsoftware.com");
        _instanceDocument.Entities.Add(entity.Identifier!, entity);

        _instanceDocument.Periods.Add("startOfYear", new XBRLPeriod()
        {
            Id = "startOfYear",
            PeriodInstantDate = "2026-01-01",
            PeriodType = Constants.XBRLInstantPeriodType
        });

        // Agregar hechos al documento instancia
        var fact = new XBRLFact(
            "f-1",
            "ifrs-full:CashAndCashEquivalents",
            _instanceDocument.Periods["startOfYear"],
            "2HSoftware",
            "52945262000");

        fact.Unit = new XBRLUnit()
        {
            Type = JBRLConstants.UnitTypeMeasure,
            Id = "u-0",
            Multipliers = new List<XBRLUnitMeasure>()
            {
                new XBRLUnitMeasure()
                {
                    Id = "iso4217:USD",
                    NameSpace = "iso4217",
                    Name = "USD"
                }
            }
        };

        _instanceDocument.Units.Add(fact.Unit.Id, fact.Unit);
        _instanceDocument.Facts.Add(fact.Id, fact);

        // Agregar hechos con dimensiones
        foreach (var member in productMembers)
        {
            var revenueFact = new XBRLFact(
                $"f-revenue-{member}",
                "ifrs-full:Revenue",
                _instanceDocument.Periods["startOfYear"],
                "2HSoftware",
                (Random.Shared.Next(1000, 10000) * 1000).ToString());

            revenueFact.Unit = _instanceDocument.Units["u-0"];
            revenueFact.Dimensions = new List<XBRLFactDimension>
            {
                new XBRLFactDimension("dim:ProductType", $"mem:{member}")
            };

            _instanceDocument.Facts.Add(revenueFact.Id, revenueFact);
        }

        _instanceDocument.PrepareFactsContextsAndUnits();

        // Configurar el motor de plantillas
        _configuration = new TemplateConfiguration
        {
            TemplateNamespacePrefix = "hh",
            DefaultNumberFormat = "#,##0.00",
            DefaultDateFormat = "yyyy-MM-dd",
            CultureInfo = CultureInfo.InvariantCulture,
            MaxNestingLevel = 10,
            MaxIterations = 1000,
            StrictValidation = true,

        };

        // Registrar formateadores personalizados
        _configuration.ValueFormatters["ixt:num-dot-decimal"] = new NumDotDecimalFormatter();
        _configuration.ValueFormatters["ixt:dateslasheu"] = new DateFormatter("dd/MM/yyyy");

        var factFinder = new XbrlFactFinder(_instanceDocument, _taxonomy.Concepts);
        _processor = new TemplateProcessor(_configuration, factFinder, _taxonomy, _instanceDocument);
    }

    [Test]
    public async Task ProcessTemplate_WithDimensionIteration_GeneratesCorrectIXBRL()
    {
        // Arrange
        var template = @"<?xml version='1.0' encoding='UTF-8'?>
        <html xmlns='http://www.w3.org/1999/xhtml' 
              xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
              xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
            <head><title>Revenue by Product Type</title></head>
            <body>
                <div style='display:hidden;'>
                    <ix:header>
                        <ix:hidden></ix:hidden>
                        <ix:references></ix:references>
                        <ix:resources></ix:resources>
                    </ix:header>
                </div>
                <h2>Revenue by Product Type</h2>
                <table border='1'>
                    <tr>
                        <th>Index</th>
                        <th>Product Type</th>
                        <th>Revenue</th>
                    </tr>
                    <hh:xbrl-iterate-dimension 
                        dimension='dim:ProductType' 
                        member-var='product' 
                        index-var='idx'>
                        <hh:xbrl-if condition=""${product} != 'mem:Training'"" else-id=""training-omitted"">
                            <tr>
                                <td><hh:xbrl-variable name=""idx"" default=""-1"" /></td>
                                <td><hh:xbrl-label member=""${product}"" /></td>
                                <td>
                                    <hh:xbrl-fact 
                                        concept='ifrs-full:Revenue'
                                        periods='[""startOfYear""]'
                                        dimensions='{""dim:ProductType"": ""${product}""}'
                                        format='ixt:numdotdecimal'/>
                                </td>
                            </tr>
                        </hh:xbrl-if>
                        <hh:xbrl-else id=""training-omitted"">
                            <tr>
                                <td colspan=""3"">
                                    Training revenue is omitted from this report
                                </td>
                            </tr>
                        </hh:xbrl-else>
                    </hh:xbrl-iterate-dimension>
                </table>
            </body>
        </html>";

        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);

        // Verificar que todas las dimensiones excepto Training están presentes
        var factNodes = result.SelectNodes(
            "//ix:nonFraction[@name='ifrs-full:Revenue']",
            CreateNamespaceManager(result));
        Assert.IsNotNull(factNodes);
        Assert.AreEqual(3, factNodes.Count); // Solo debería haber 3 hechos (excluyendo Training)

        // Verificar que el mensaje de omisión está presente
        var omissionMessage = result.SelectSingleNode(
            "//xhtml:tr/xhtml:td[@colspan='3' and contains(normalize-space(), 'Training revenue is omitted from this report')]",
            CreateNamespaceManager(result));
        Assert.IsNotNull(omissionMessage, "Mensaje de omisión no encontrado");

        // Verificar que el hecho de Training no está presente
        var trainingFact = result.SelectSingleNode(
            "//ix:nonFraction[contains(@contextRef, 'Training')]",
            CreateNamespaceManager(result));
        Assert.IsNull(trainingFact, "El hecho de Training no debería estar presente");

        Debug.WriteLine(result.OuterXml);
    }

    private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
    {
        var nsManager = new XmlNamespaceManager(doc.NameTable);
        nsManager.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
        nsManager.AddNamespace("hh", "http://www.2hsoftware.com.mx/ixbrl/template/hh");
        nsManager.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        return nsManager;
    }
}
