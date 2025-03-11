using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCore.Dto.InstanceEditor.Dto;
using AbaxXBRLRealTime.Shared;
using Newtonsoft.Json;
using Abax2InlineXBRLGenerator.Generator.Formatters;

namespace TestAbax2InlineXBRLGenerator;

public class IXBRLVariablesTemplateTests
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


        var unitUSD = new XBRLUnit()
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

        _instanceDocument.Units.Add(unitUSD.Id, unitUSD);

        var entity = new XBRLEntity("2HSoftware", "http://www.2hsoftware.com");
        _instanceDocument.Entities.Add(entity.Identifier!, entity);

        SetupFinancialStatementFacts();

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

    private void SetupFinancialStatementFacts()
    {
        // Agregar conceptos necesarios a la taxonomía
        var concepts = new[]
        {
            ("ifrs-full:Assets", "Assets", true),
            ("ifrs-full:CurrentAssets", "Current Assets", true),
            ("ifrs-full:CashAndCashEquivalents", "Cash and Cash Equivalents", false),
            ("ifrs-full:TradeAndOtherCurrentReceivables", "Trade Receivables", false),
            ("ifrs-full:Inventories", "Inventories", false),
            ("ifrs-full:NoncurrentAssets", "Non-current Assets", true),
            ("ifrs-full:PropertyPlantAndEquipment", "Property, Plant and Equipment", false),
            ("ifrs-full:IntangibleAssetsOtherThanGoodwill", "Intangible Assets", false)
        };

        foreach (var (id, name, isAbstract) in concepts)
        {
            _taxonomy.Concepts[id] = new XBRLConcept(
                id,
                name,
                "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
                Constants.XBRLInstantPeriodType,
                "debit",
                "xbrli:monetaryItemType",
                "xbrli:item",
                ConceptoDto.Item,
                isAbstract,
                false,
                true);
        }

        _taxonomy.Concepts["ifrs-full:Revenue"] = new XBRLConcept(
                "ifrs-full:Revenue",
                "Revenue",
                "http://xbrl.ifrs.org/taxonomy/2024-01-01/ifrs-full",
                Constants.XBRLDurationPeriodType,
                "debit",
                "xbrli:monetaryItemType",
                "xbrli:item",
                ConceptoDto.Item,
                false,
                false,
                true);

        // Agregar hechos con diferentes valores
        var facts = new[]
        {
            ("ifrs-full:Assets", 10000000m),
            ("ifrs-full:CurrentAssets", 6000000m),
            ("ifrs-full:CashAndCashEquivalents", 2500000m),
            ("ifrs-full:TradeAndOtherCurrentReceivables", 2000000m),
            ("ifrs-full:Inventories", 500000m),
            ("ifrs-full:NoncurrentAssets", 4000000m),
            ("ifrs-full:PropertyPlantAndEquipment", 3000000m),
            ("ifrs-full:IntangibleAssetsOtherThanGoodwill", 1000000m)
        };

        foreach (var (concept, value) in facts)
        {
            var fact = new XBRLFact(
                $"f-{concept}",
                concept,
                _instanceDocument.Periods["startOfYear"],
                "2HSoftware",
                value.ToString(CultureInfo.InvariantCulture));

            fact.Unit = _instanceDocument.Units["u-0"];
            _instanceDocument.Facts.Add(fact.Id, fact);
        }

        _instanceDocument.PrepareFactsContextsAndUnits();
    }

    [Test]
    public async Task ProcessTemplate_WithPresentationLinkbase_GeneratesCorrectStructure()
    {
        // Arrange
        // Configurar el presentation linkbase en la taxonomía
        _taxonomy.PresentationLinkbases["http://example.com/role/BalanceSheet"] =
            new XBRLPresentationLinkbase("NAme", "http://example.com/role/BalanceSheet");
        _taxonomy.PresentationLinkbases["http://example.com/role/BalanceSheet"].PresentationLinkbaseItems.AddRange(
            new []{
            new XBRLPresentationLinkbaseItem
            {
                ConceptId = "ifrs-full:Assets",
                LabelRole = "http://www.xbrl.org/2003/role/label",
                Indentation = 0,
                IsAbstract = true
            },
            new XBRLPresentationLinkbaseItem
            {
                ConceptId = "ifrs-full:CurrentAssets",
                LabelRole = "http://www.xbrl.org/2003/role/label",
                Indentation = 1,
                IsAbstract = true
            },
            new XBRLPresentationLinkbaseItem
            {
                ConceptId = "ifrs-full:CashAndCashEquivalents",
                LabelRole = "http://www.xbrl.org/2003/role/label",
                Indentation = 2,
                IsAbstract = false
            }});

        var template = @"<?xml version='1.0' encoding='UTF-8'?>
        <html xmlns='http://www.w3.org/1999/xhtml' 
              xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
              xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
            <head><title>Balance Sheet Structure</title></head>
            <body>
                <div style='display:none;'>
                    <ix:header>
                        <ix:hidden></ix:hidden>
                        <ix:references></ix:references>
                        <ix:resources></ix:resources>
                    </ix:header>
                </div>

                <hh:xbrl-presentation-linkbase 
                    role='http://example.com/role/BalanceSheet'
                    name='balanceSheet'
                    startFrom='ifrs-full:CurrentAssets' />
                
                <table border='1'>
                    <thead>
                        <tr>
                            <th>Concept</th>
                            <th>Type</th>
                            <th>Indentation</th>
                        </tr>
                    </thead>
                    <tbody>
                        <hh:xbrl-iterate-variable variable='balanceSheet' item-var='item'>
                            <tr>
                                <td>
                                    <hh:xbrl-label concept='${item.conceptId}'/>
                                </td>
                                <td>${item.isAbstract ? 'Abstract' : 'Monetary'}</td>
                                <td>${item.indentation}</td>
                            </tr>
                        </hh:xbrl-iterate-variable>
                    </tbody>
                </table>
            </body>
        </html>";

        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);

        Debug.WriteLine(result.OuterXml);

        // Verificar que solo se procesaron los items desde CurrentAssets
        var rows = result.SelectNodes("//xhtml:tr[xhtml:td]", CreateNamespaceManager(result));
        Assert.IsNotNull(rows);
        Assert.AreEqual(2, rows.Count); // CurrentAssets y CashAndCashEquivalents

        Debug.WriteLine(result.OuterXml);
    }

    [Test]
    public async Task ProcessTemplate_WithVariablesAndIndentation_GeneratesStructuredFinancialStatement()
    {
        // Define la plantilla
        var template = @"<?xml version='1.0' encoding='UTF-8'?>
                        <html xmlns='http://www.w3.org/1999/xhtml' 
                            xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
                            xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
                            <head><title>Estado de Situación Financiera</title>
                                <style>
                                    .indent-1 { padding-left: 20px; }
                                    .indent-2 { padding-left: 40px; }
                                    .total { font-weight: bold; }
                                </style>
                            </head>
                            <body>
                                <div style='display:none;'>
                                    <ix:header>
                                        <ix:hidden></ix:hidden>
                                        <ix:references></ix:references>
                                        <ix:resources></ix:resources>
                                    </ix:header>
                                </div>

                                <!-- Definición de variables -->
                                <hh:xbrl-set-variable name=""show_details"" value=""true"" type=""boolean"" />
                                <hh:xbrl-set-variable name=""min_value"" value=""1000000"" type=""double"" />
                                <hh:xbrl-set-variable name=""report_title"" value=""Estado de Situación Financiera"" type=""string"" />
                                <hh:xbrl-set-variable name=""statement_structure"" value='[
                                    {""concept"": ""ifrs-full:Assets"", ""indent"": 0, ""isTotal"": true},
                                    {""concept"": ""ifrs-full:CurrentAssets"", ""indent"": 1, ""isTotal"": true},
                                    {""concept"": ""ifrs-full:CashAndCashEquivalents"", ""indent"": 2, ""isTotal"": false},
                                    {""concept"": ""ifrs-full:TradeAndOtherCurrentReceivables"", ""indent"": 2, ""isTotal"": false},
                                    {""concept"": ""ifrs-full:Inventories"", ""indent"": 2, ""isTotal"": false},
                                    {""concept"": ""ifrs-full:NoncurrentAssets"", ""indent"": 1, ""isTotal"": true},
                                    {""concept"": ""ifrs-full:PropertyPlantAndEquipment"", ""indent"": 2, ""isTotal"": false},
                                    {""concept"": ""ifrs-full:IntangibleAssetsOtherThanGoodwill"", ""indent"": 2, ""isTotal"": false}
                                ]' type=""dictionary-array"" />

                                <h1><hh:xbrl-variable name=""report_title"" /></h1>
                                <table border='1' style='width: 100%'>
                                    <thead>
                                        <tr>
                                            <th>Concepto</th>
                                            <th>Valor</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <hh:xbrl-iterate-variable variable=""statement_structure"" item-var=""line_item"">
                                            <hh:xbrl-set-variable name=""current_value"" fact-filter='{""conceptId"": ""${line_item.concept}"", ""period"": ""startOfYear""}' />
                                            <hh:xbrl-if condition=""${show_details} || ${line_item.isTotal}"">
                                                <hh:xbrl-if condition=""${current_value} >= ${min_value}"" else-id=""below-threshold"">
                                                    <tr>
                                                        <td class='indent-${line_item.indent}'>
                                                            <hh:xbrl-label concept=""${line_item.concept}"" />
                                                        </td>
                                                        <td style='text-align: right;' class='${line_item.isTotal ? ""total"" : """"}'>
                                                            <hh:xbrl-fact 
                                                                concept=""${line_item.concept}""
                                                                periods=""['startOfYear']""
                                                                format=""ixt:num-dot-decimal"" />
                                                        </td>
                                                    </tr>
                                                </hh:xbrl-if>
                                                <hh:xbrl-else id=""below-threshold"">
                                                    <tr>
                                                        <td class='indent-${line_item.indent}'>
                                                            <hh:xbrl-label concept=""${line_item.concept}"" />
                                                        </td>
                                                        <td style='text-align: right;'>
                                                            <em>Monto inferior al umbral mínimo</em>
                                                        </td>
                                                    </tr>
                                                </hh:xbrl-else>
                                            </hh:xbrl-if>
                                        </hh:xbrl-iterate-variable>
                                    </tbody>
                                </table>
                            </body>
                        </html>";

        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);

        Debug.WriteLine(result.OuterXml);

        // Verificar la estructura básica
        var tableRows = result.SelectNodes("//xhtml:tr", CreateNamespaceManager(result));
        Assert.IsNotNull(tableRows, "No se encontraron filas en la tabla");
        Assert.Greater(tableRows.Count, 1, "No hay suficientes filas en la tabla");

        // Verificar que los totales están en negrita
        var totalRows = result.SelectNodes("//xhtml:tr/xhtml:td[@class='total']", CreateNamespaceManager(result));
        Assert.IsNotNull(totalRows, "No se encontraron filas de totales");

        // Verificar las indentaciones
        var indentedCells = result.SelectNodes("//xhtml:td[contains(@class, 'indent-')]", CreateNamespaceManager(result));
        Assert.IsNotNull(indentedCells, "No se encontraron celdas con indentación");
    }

    [Test]
    public async Task ProcessTemplate_WithStringArrayIteration_GeneratesCorrectOutput()
    {
        // Arrange
        // Configurar una variable de tipo string array
        var periodsArray = new string[] { "2024", "2023", "2022" };
        var initialVariables = new Dictionary<string, string>
        {
            ["availablePeriods"] = JsonConvert.SerializeObject(periodsArray)
        };

        var template = @"<?xml version='1.0' encoding='UTF-8'?>
       <html xmlns='http://www.w3.org/1999/xhtml' 
             xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
             xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
           <head><title>Period Analysis</title></head>
           <body>
               <div style='display:none;'>
                   <ix:header>
                       <ix:hidden></ix:hidden>
                       <ix:references></ix:references>
                       <ix:resources></ix:resources>
                   </ix:header>
               </div>

               <!-- Definir la variable como StringArray -->
               <hh:xbrl-set-variable name=""periods"" 
                                   value=""${availablePeriods}"" 
                                   type=""String-Array"" />

               <table border='1'>
                   <thead>
                       <tr>
                           <th>Period</th>
                           <th>Assets</th>
                       </tr>
                   </thead>
                   <tbody>
                       <hh:xbrl-iterate-variable variable='periods' 
                                                item-var='period' 
                                                index-var='idx'>
                           <tr>
                               <td>Year ${period}</td>
                               <td>
                                   <hh:xbrl-fact concept='ifrs-full:Assets' 
                                               periods='[""${period}""]'
                                               format='ixt:num-dot-decimal'/>
                               </td>
                           </tr>
                       </hh:xbrl-iterate-variable>
                   </tbody>
               </table>
           </body>
       </html>";

        // Crear los períodos en el documento instancia
        foreach (var period in periodsArray)
        {
            _instanceDocument.Periods.Add(period, new XBRLPeriod
            {
                Id = period,
                PeriodType = Constants.XBRLInstantPeriodType,
                PeriodInstantDate = $"{period}-12-31"  // o la fecha que corresponda
            });
        }

        // Agregar hechos para cada período
        foreach (var period in periodsArray)
        {
            var fact = new XBRLFact(
                $"f-assets-{period}",
                "ifrs-full:Assets",
                _instanceDocument.Periods[period],
                "2HSoftware",
                (100000000 * (1 + int.Parse(period) - 2022)).ToString());  // Valor diferente para cada año

            fact.Unit = _instanceDocument.Units["u-0"];
            _instanceDocument.Facts.Add(fact.Id, fact);
        }

        _instanceDocument.PrepareFactsContextsAndUnits();

        var factFinder = new XbrlFactFinder(_instanceDocument, _taxonomy.Concepts);
        _processor = new TemplateProcessor(_configuration, factFinder, _taxonomy, _instanceDocument);
        // Act
        var result = await _processor.ProcessTemplate(template, initialVariables);

        // Assert
        Assert.IsNotNull(result);

        // Verificar que se crearon todas las filas
        var rows = result.SelectNodes("//xhtml:tr[xhtml:td[contains(text(), 'Year')]]", CreateNamespaceManager(result));
        Assert.IsNotNull(rows, "No se encontraron filas en la tabla");
        Assert.AreEqual(3, rows.Count, "No se generaron todas las filas esperadas");

        // Verificar que cada período está presente y tiene un valor
        foreach (var period in periodsArray)
        {
            var periodCell = result.SelectSingleNode(
                $"//xhtml:td[contains(text(), 'Year {period}')]",
                CreateNamespaceManager(result));
            Assert.IsNotNull(periodCell, $"No se encontró la celda para el año {period}");

            var factNode = result.SelectSingleNode(
                $"//xhtml:tr[xhtml:td[contains(text(), 'Year {period}')]]//ix:nonFraction",
                CreateNamespaceManager(result));
            Assert.IsNotNull(factNode, $"No se encontró el hecho XBRL para el año {period}");
        }

        Debug.WriteLine(result.OuterXml);
    }

    [Test]
    public async Task ProcessTemplate_WithPeriodArray_HandlesMultiplePeriodTypes()
    {
        // Arrange
        // Configurar periodos de tipo instant
        _instanceDocument.Periods.Add("endOfYear2024", new XBRLPeriod
        {
            Id = "endOfYear2024",
            PeriodType = Constants.XBRLInstantPeriodType,
            PeriodInstantDate = "2024-12-31"
        });
        _instanceDocument.Periods.Add("endOfYear2023", new XBRLPeriod
        {
            Id = "endOfYear2023",
            PeriodType = Constants.XBRLInstantPeriodType,
            PeriodInstantDate = "2023-12-31"
        });

        // Configurar periodos de tipo duration
        _instanceDocument.Periods.Add("year2024", new XBRLPeriod
        {
            Id = "year2024",
            PeriodType = "duration",
            PeriodStartDate = "2024-01-01",
            PeriodEndDate = "2024-12-31"
        });
        _instanceDocument.Periods.Add("year2023", new XBRLPeriod
        {
            Id = "year2023",
            PeriodType = "duration",
            PeriodStartDate = "2023-01-01",
            PeriodEndDate = "2023-12-31"
        });

        // Agregar hechos con diferentes tipos de periodos
        var facts = new[]
        {
            new
            {
                Id = "f-assets-2024-instant",
                Concept = "ifrs-full:Assets",
                Period = "endOfYear2024",
                Value = "120000000"
            },
            new
            {
                Id = "f-assets-2023-instant",
                Concept = "ifrs-full:Assets",
                Period = "endOfYear2023",
                Value = "100000000"
            },
            new
            {
                Id = "f-revenue-2024",
                Concept = "ifrs-full:Revenue",
                Period = "year2024",
                Value = "50000000"
            },
            new
            {
                Id = "f-revenue-2023",
                Concept = "ifrs-full:Revenue",
                Period = "year2023",
                Value = "40000000"
            }
        };

        foreach (var factData in facts)
        {
            var fact = new XBRLFact(
                factData.Id,
                factData.Concept,
                _instanceDocument.Periods[factData.Period],
                "2HSoftware",
                factData.Value);
            fact.Unit = _instanceDocument.Units["u-0"];
            _instanceDocument.Facts.Add(fact.Id, fact);
        }

        _instanceDocument.PrepareFactsContextsAndUnits();

        var template = @"<?xml version='1.0' encoding='UTF-8'?>
        <html xmlns='http://www.w3.org/1999/xhtml' 
              xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
              xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
            <head><title>Period Types Test</title></head>
            <body>
                <div style='display:none;'>
                    <ix:header>
                        <ix:hidden></ix:hidden>
                        <ix:references></ix:references>
                        <ix:resources></ix:resources>
                    </ix:header>
                </div>
                <table>
                    <!-- Prueba 1: Instant a Instant -->
                    <tr>
                        <td>Instant periods:</td>
                        <td>
                            <hh:xbrl-fact concept='ifrs-full:Assets' 
                                        periods='${matchPeriod(""endOfYear2024"", ""end"")}' 
                                        format='ixt:num-dot-decimal'/>
                        </td>
                    </tr>
                    <!-- Prueba 2: Duration a Duration -->
                    <tr>
                        <td>Duration periods:</td>
                        <td>
                            <hh:xbrl-fact concept='ifrs-full:Revenue' 
                                        periods='${matchPeriod(""year2024"", ""end"")}' 
                                        format='ixt:num-dot-decimal'/>
                        </td>
                    </tr>
                    <!-- Prueba 3: Duration a Instant -->
                    <tr>
                        <td>Duration end to Instant:</td>
                        <td>
                            <hh:xbrl-fact concept='ifrs-full:Assets' 
                                        periods='${matchPeriod(""year2024"", ""end"")}' 
                                        format='ixt:num-dot-decimal'/>
                        </td>
                    </tr>
                    <!-- Prueba 4: Instant a Duration -->
                    <tr>
                        <td>Instant to Duration end:</td>
                        <td>
                            <hh:xbrl-fact concept='ifrs-full:Revenue' 
                                        periods='${matchPeriod(""endOfYear2024"", ""end"")}' 
                                        format='ixt:num-dot-decimal'/>
                        </td>
                    </tr>
                    <!-- Prueba 5: Duration start match -->
                    <tr>
                        <td>Duration start match:</td>
                        <td>
                            <hh:xbrl-fact concept='ifrs-full:Revenue' 
                                        periods='${matchPeriod(""year2024"", ""start"")}' 
                                        format='ixt:num-dot-decimal'/>
                        </td>
                    </tr>
                </table>
            </body>
        </html>";

        var factFinder = new XbrlFactFinder(_instanceDocument, _taxonomy.Concepts);
        _processor = new TemplateProcessor(_configuration, factFinder, _taxonomy, _instanceDocument);
        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);

        // Helper function para verificar hechos
        void VerifyFact(int rowIndex, string expectedValue, string description)
        {
            var fact = result.SelectSingleNode(
                $"//xhtml:tr[{rowIndex}]//ix:nonFraction",
                CreateNamespaceManager(result));
            Assert.IsNotNull(fact, $"No se encontró el hecho para {description}");
            Assert.AreEqual(_configuration.ValueFormatters["ixt:num-dot-decimal"].Format(expectedValue), fact.InnerText.Trim(),
                $"Valor incorrecto para {description}");
        }

        // Verificar cada caso
        VerifyFact(1, "120000000", "coincidencia instant a instant");
        VerifyFact(2, "50000000", "coincidencia duration a duration");
        VerifyFact(3, "120000000", "coincidencia duration end a instant");
        VerifyFact(4, "50000000", "coincidencia instant a duration end");
        VerifyFact(5, "50000000", "coincidencia por fecha de inicio");

        Debug.WriteLine(result.OuterXml);
    }

    [Test]
    public async Task ProcessTemplate_WithDateOfPeriod_FormatsDateCorrectly()
    {
        // Arrange
        // Configurar periodos
        _instanceDocument.Periods.Add("endOfYear", new XBRLPeriod
        {
            Id = "endOfYear",
            PeriodType = Constants.XBRLInstantPeriodType,
            PeriodInstantDate = "2024-12-31"
        });

        _instanceDocument.Periods.Add("currentQuarter", new XBRLPeriod
        {
            Id = "currentQuarter",
            PeriodType = "duration",
            PeriodStartDate = "2024-01-01",
            PeriodEndDate = "2024-03-31"
        });

        var template = @"<?xml version='1.0' encoding='UTF-8'?>
        <html xmlns='http://www.w3.org/1999/xhtml' 
              xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
              xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
            <body>
                <div style='display:none;'>
                    <ix:header>
                        <ix:hidden></ix:hidden>
                        <ix:references></ix:references>
                        <ix:resources></ix:resources>
                    </ix:header>
                </div>
                <div>
                    <p>Fecha fin de año: ${dateOfPeriod(""endOfYear"", ""dd/MM/yyyy"")}</p>
                    <p>Inicio trimestre: ${dateOfPeriod(""currentQuarter"", ""MM/yyyy"", ""startDate"")}</p>
                    <p>Fin trimestre: ${dateOfPeriod(""currentQuarter"", ""dd-MM"", ""endDate"")}</p>
                </div>
            </body>
        </html>";

        // Act
        var result = await _processor.ProcessTemplate(template);

        // Assert
        Assert.IsNotNull(result);

        Debug.WriteLine(result.OuterXml);

        var paragraphs = result.SelectNodes("//xhtml:p", CreateNamespaceManager(result));
        Assert.IsNotNull(paragraphs);
        Assert.That(paragraphs[0].InnerText, Does.Contain("31/12/2024"));
        Assert.That(paragraphs[1].InnerText, Does.Contain("01/2024"));
        Assert.That(paragraphs[2].InnerText, Does.Contain("31-03"));
    }

    [Test]
    public async Task ProcessTemplate_WithArithmeticOperations_CalculatesCorrectly()
    {
        // Arrange
        var initialVariables = new Dictionary<string, string>
        {
            ["numberA"] = "10",
            ["numberB"] = "5",
            ["text1"] = "Hello ",
            ["text2"] = "World"
        };

        var template = @"<?xml version='1.0' encoding='UTF-8'?>
        <html xmlns='http://www.w3.org/1999/xhtml' 
              xmlns:ix='http://www.xbrl.org/2013/inlineXBRL'
              xmlns:hh='http://www.2hsoftware.com.mx/ixbrl/template/hh'>
            <body>
                <div style='display:none;'>
                    <ix:header>
                        <ix:hidden></ix:hidden>
                        <ix:references></ix:references>
                        <ix:resources></ix:resources>
                    </ix:header>
                </div>
                <div>
                    <!-- Operaciones aritméticas básicas -->
                    <p>Suma: ${${numberA} + ${numberB}}</p>
                    <p>Resta: ${${numberA} - ${numberB}}</p>
                    <p>Multiplicación: ${${numberA} * ${numberB}}</p>
                    <p>División: ${${numberA} / ${numberB}}</p>
                    <p>Módulo: ${${numberA} % ${numberB}}</p>
                    <p>Potencia: ${${numberA} ^ 2}</p>

                    <!-- Conversión a double -->
                    <p>Convertido: ${toDouble('123.45') + ${numberA}}</p>

                    <!-- Concatenación de strings -->
                    <p>Concatenado: ${${text1} + "" "" + ${text2}}</p>

                    <!-- Uso mixto -->
                    <p>Mixto: ${toDouble(${numberA}) + 5.5}</p>
                </div>
            </body>
        </html>";

        // Act
        var result = await _processor.ProcessTemplate(template, initialVariables);

        // Assert
        Assert.IsNotNull(result);

        Debug.WriteLine(result.OuterXml);
        
        var paragraphs = result.SelectNodes("//xhtml:p", CreateNamespaceManager(result));
        Assert.That(paragraphs[0].InnerText, Does.Contain("15"));         // 10 + 5
        Assert.That(paragraphs[1].InnerText, Does.Contain("5"));          // 10 - 5
        Assert.That(paragraphs[2].InnerText, Does.Contain("50"));         // 10 * 5
        Assert.That(paragraphs[3].InnerText, Does.Contain("2"));          // 10 / 5
        Assert.That(paragraphs[4].InnerText, Does.Contain("0"));          // 10 % 5
        Assert.That(paragraphs[5].InnerText, Does.Contain("100"));        // 10 ^ 2
        Assert.That(paragraphs[6].InnerText, Does.Contain("133.45"));     // 123.45 + 10
        Assert.That(paragraphs[7].InnerText, Does.Contain("Hello World")); // concatenación
        Assert.That(paragraphs[8].InnerText, Does.Contain("15.5"));       // 10 + 5.5

        
    }

    private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
    {
        var nsManager = new XmlNamespaceManager(doc.NameTable);
        nsManager.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
        nsManager.AddNamespace("hh", _configuration.TemplateNamespace);
        nsManager.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        return nsManager;
    }
}
