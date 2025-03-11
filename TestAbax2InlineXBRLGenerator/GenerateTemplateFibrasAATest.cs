using System.Diagnostics;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator.Impl;
using TestAbax2InlineXBRLGenerator.Base;

namespace TestAbax2InlineXBRLGenerator;

public class GenerateTemplateFibrasAATest : GenerateIXbrlBaseTest
{
    [SetUp]
    public void Setup()
    {
        init();
    }

    [Test]
    public async Task ProcessTemplate_FromFile_GeneratesAndSavesIXBRLDocument()
    {
        // 80678 - FIBRAPL 2024-3
        var realTimeInstance = await LoadJbrlDocumentByFilingId("80678");
        var entryPointHref = await GetEntryPointHref(realTimeInstance.TaxonomyId);
        var renderer = new IFRS2019FibrasAAInlineXBRLRenderer();

        var sw = new Stopwatch();
        sw.Start();

        // Arrange
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "fibras_aa_2019_template.html");
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Output", "fibras_aa-2019-output.html");

        // Asegurar que existe el directorio de salida
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        try
        {
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