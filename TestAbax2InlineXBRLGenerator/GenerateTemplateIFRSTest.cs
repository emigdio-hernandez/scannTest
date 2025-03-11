using System.Diagnostics;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator.Impl;
using TestAbax2InlineXBRLGenerator.Base;

namespace TestAbax2InlineXBRLGenerator;

public class GenerateTemplateIFRSTest : GenerateIXbrlBaseTest
{

    [SetUp]
    public void Setup()
    {
        init();
    }

    [Test]
    public async Task ProcessTemplate_UsingIFRS2019ICSRenderer_GeneratesAndSavesIXBRLDocument()
    {
        //80549
        
        //36756 - Pemex 4D
        
        //81278 - PROCORP 2023-3
        
        //81283 PROCORP 2023-4D
        
        //81285 PROCORP - 1

        //80246 Notas al pie de página 210000 BMWX 2024-3
        
        //31742 reexpresión de capital KOF

        var realTimeInstance = await LoadJbrlDocumentByFilingId("31742");
        var entryPointHref = await GetEntryPointHref(realTimeInstance.TaxonomyId);
        var renderer = new IFRS2019ICSInlineXBRLRenderer();
        
        var sw = new Stopwatch();
        sw.Start();

        // Arrange template paths
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "ics_2019_template.html");
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Output", "ics-2019-output-new.html");

        // Ensure output directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        try
        {
            // Act
            var templateContent = File.ReadAllText(templatePath);
            var result = await renderer.RenderInlineXBRL(templateContent, realTimeInstance, entryPointHref);
            sw.Stop();
            Debug.WriteLine("Time to process template using new renderer:" + sw.ElapsedMilliseconds);
            // Save the result
            using var writer = XmlWriter.Create(outputPath, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                ConformanceLevel = ConformanceLevel.Document
            });

            writer.WriteDocType("html", null, "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", null);
            result.Save(writer);
        }
        catch (Exception ex)
        {
            string innerMessage = ex.InnerException?.Message ?? string.Empty;
            Debug.WriteLine(ex.InnerException?.Message);
            Assert.Fail($"Test failed with error: {ex.Message}{(innerMessage != string.Empty ? " - " + innerMessage : "")}");
        }
    }

}