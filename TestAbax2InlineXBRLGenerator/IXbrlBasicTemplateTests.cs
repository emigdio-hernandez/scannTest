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

public class IXbrlBasicTemplateTests
{

    private TemplateProcessor _processor;
    private XBRLInstanceDocument _instanceDocument;
    private XBRLTaxonomy _taxonomy;
    private TemplateConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        // Configurar la taxonomía
        _taxonomy = new XBRLTaxonomy(
            "ifrs-full",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "IFRS Full",
            "2024-01-01");

        _taxonomy.Prefixes = new Dictionary<string, string>
        {
            { "ifrs-full", "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full" },
            { "iso4217", "http://www.xbrl.org/2003/iso4217" }
        };

        // Agregar conceptos a la taxonomía
        _taxonomy.Concepts["ifrs-full:CashAndCashEquivalents"] = new XBRLConcept(
            "ifrs-full:CashAndCashEquivalents",
            "CashAndCashEquivalents",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            Constants.XBRLInstantPeriodType,
            "credit",
            "http://www.xbrl.org/2003/instance:monetaryItemType",
            "http://www.xbrl.org/2003/instance:monetaryItemType",
            1,
            false,
            false,
            true);

        // Configurar el documento instancia
        _instanceDocument = new XBRLInstanceDocument(
            "test-instance",
            "Test Instance",
            "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
            "2HSoftware", _taxonomy,null);

        _instanceDocument.Periods.Add("startOfYear", new XBRLPeriod()
        {
            Id = "startOfYear",
            PeriodInstantDate = "2026-01-01",
            PeriodType = Constants.XBRLInstantPeriodType
        });

        var entity = new XBRLEntity("2HSoftware", "http://www.2hsoftware.com");
        _instanceDocument.Entities.Add(entity.Identifier!, entity);

        // Agregar hechos al documento instancia
        var fact = new XBRLFact(
            "f-1",
            "ifrs-full:CashAndCashEquivalents",
            _instanceDocument.Periods["startOfYear"],
            "2HSoftware",
            "52945262000");
        
        fact.Unit = new XBRLUnit(){
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
    public async Task ProcessTemplate_WithSimpleFact_GeneratesCorrectIXBRL()
    {
        // Arrange
        var template = @"<?xml version='1.0' encoding='UTF-8'?>
            <html xmlns='http://www.w3.org/1999/xhtml' 
                  xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
                  xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
                <head><title>XBRL Report</title></head>
                <body>
                    <div style='display:hidden;'>
                        <ix:header>
                            <ix:hidden></ix:hidden>
                            <ix:references></ix:references>
                            <ix:resources></ix:resources>
                        </ix:header>
                    </div>
                    <div>
                        <hh:xbrl-fact concept='ifrs-full:CashAndCashEquivalents' 
                                    periods='[""startOfYear""]'
                                    format='ixt:num-dot-decimal'/>
                    </div>
                </body>
            </html>";

        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);
        
        Debug.WriteLine(result.OuterXml);

        // Verificar la existencia del contexto
        var contextNode = result.SelectSingleNode(
            "//xbrli:context[@id='c-0']", 
            CreateNamespaceManager(result));
        Assert.IsNotNull(contextNode, "Contexto no encontrado");
        
        // Verificar la existencia de la unidad
        var unitNode = result.SelectSingleNode(
            "//xbrli:unit[@id='u-0']", 
            CreateNamespaceManager(result));
        Assert.IsNotNull(unitNode, "Unidad no encontrada");
        
        // Verificar el hecho XBRL
        var factNode = result.SelectSingleNode(
            "//ix:nonFraction[@name='ifrs-full:CashAndCashEquivalents']",
            CreateNamespaceManager(result));
        Assert.IsNotNull(factNode, "Hecho XBRL no encontrado");
        
        // Verificar atributos del hecho
        Assert.AreEqual("c-0", factNode.Attributes["contextRef"]?.Value);
        Assert.AreEqual("u-0", factNode.Attributes["unitRef"]?.Value);
        Assert.AreEqual("ixt:num-dot-decimal", factNode.Attributes["format"]?.Value);
        
        // Verificar el valor formateado
        Assert.AreEqual("52,945,262,000", factNode.InnerText.Trim());

        // Verificar que no existan elementos de plantilla en el resultado
        var templateNode = result.SelectSingleNode(
            "//hh:xbrl-fact", 
            CreateNamespaceManager(result));
        Assert.IsNull(templateNode, "Elemento de plantilla encontrado en el resultado");
    }

    private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
    {
        var nsManager = new XmlNamespaceManager(doc.NameTable);
        nsManager.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
        nsManager.AddNamespace("hh", _configuration.TemplateNamespace);
        nsManager.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        return nsManager;
    }

}
