using System;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Generator.Functions;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Main processor for the iXBRL template system.
/// </summary>
public class TemplateProcessor
{
    private readonly TemplateConfiguration _configuration;
    private readonly XbrlFactFinder _factFinder;
    private TemplateContext? _context;
    private TemplateParser _parser;
    private XBRLTaxonomy? _taxonomy;
    private XBRLInstanceDocument? _instanceDocument;

    /// <summary>
    /// Initializes a new instance of the TemplateProcessor class.
    /// </summary>
    public TemplateProcessor(TemplateConfiguration configuration, XbrlFactFinder factFinder, XBRLTaxonomy? taxonomy, XBRLInstanceDocument? instanceDocument)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _factFinder = factFinder ?? throw new ArgumentNullException(nameof(factFinder));
        _taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
        _parser = new TemplateParser(configuration);
        _instanceDocument = instanceDocument ?? throw new ArgumentNullException(nameof(instanceDocument));
    }

    /// <summary>
    /// Processes a template and returns the resulting iXBRL document.
    /// </summary>
    /// <param name="templateContent">The content of the template</param>
    /// <param name="variables">Optional dictionary of global variables to initialize</param>
    public async Task<XmlDocument> ProcessTemplate(string templateContent, IDictionary<string, string>? variables = null)
    {
        // Load and validate the template
        var templateDoc = _parser.ParseTemplate(templateContent);

        // Create the output document
        var outputDoc = new XmlDocument();
        _context = new TemplateContext(_configuration, outputDoc, _taxonomy, _instanceDocument);

        // Initialize global variables if provided
        if (variables != null)
        {
            foreach (var (name, value) in variables)
            {
                var variable = new TemplateVariable(
                    name,
                    TemplateVariable.VariableType.String,
                    TemplateVariable.VariableScope.Global);

                variable.SetValue(value);
                _context.SetVariableValue(name, variable);
            }
        }
        //Initialize other global variables like documentTitle or taxonomyName

        var documentTitleVar = new TemplateVariable("documentTitle", TemplateVariable.VariableType.String, TemplateVariable.VariableScope.Global);
        var taxonomyNameVar = new TemplateVariable("taxonomyName", TemplateVariable.VariableType.String, TemplateVariable.VariableScope.Global);
        documentTitleVar.SetValue(_instanceDocument.Title);
        taxonomyNameVar.SetValue(_taxonomy.Name);
        _context.SetVariableValue(documentTitleVar.Name, documentTitleVar);
        _context.SetVariableValue(taxonomyNameVar.Name, taxonomyNameVar);

        // Initialize template functions
        InitializeTemplateFunctions();

        // Initialize tag processors
        InitializeTagProcessors();

        try
        {
            // Process the template
            var result = await ProcessNode(templateDoc.DocumentElement);
            if (result != null)
                foreach (var node in result)
                    outputDoc.AppendChild(node);

            // Process the header section
            ProcessHeaderSection(outputDoc, _context);

            // Validate the output if configured
            if (_configuration.OutputConfiguration.ValidateOutput)
            {
                ValidateOutput(outputDoc);
            }

            if (outputDoc.DocumentElement != null)
                RemoveExcessiveNewlines(outputDoc.DocumentElement);

            return outputDoc;
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.InnerException?.Message);
            Console.WriteLine(ex.InnerException?.StackTrace);
            Console.WriteLine(ex.Data);
            Console.WriteLine(ex.Source);
            Console.WriteLine(ex.TargetSite);
            Console.WriteLine(ex.HelpLink);
            Console.WriteLine(ex.HResult);
            Console.WriteLine(ex.Data);
            throw;
        }
        finally
        {
            _context = null;
        }
    }

    private void RemoveExcessiveNewlines(XmlNode node)
    {
        // Iterar sobre todos los nodos hijos
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Text)
            {
                // Limpiar el contenido del nodo de texto
                if (child.Value != null)
                    child.Value = Regex.Replace(child.Value, @"\s*\n\s*", "\n").Trim();
            }
            else if (child.NodeType == XmlNodeType.Element)
            {
                // Recursivamente limpiar los hijos
                RemoveExcessiveNewlines(child);
            }
        }
    }

    private void InitializeTemplateFunctions()
    {
        if (_context == null)
            throw new InvalidOperationException("Context not initialized");

        // Register all template functions
        _context.RegisterFunction(new MatchPeriodFunction(_context));
        _context.RegisterFunction(new DateOfPeriodFunction(_context));
        _context.RegisterFunction(new ToDoubleFunction());
        _context.RegisterFunction(new GetPresentationLinkbaseFunction(_taxonomy!));
        _context.RegisterFunction(new PostHierarchicalOrderFunction());
        _context.RegisterFunction(new CountFactsFunction(_factFinder, _context));
        _context.RegisterFunction(new FindFootnotesFunction(_factFinder, _context));
        _context.RegisterFunction(new CompareDateFunction());
        _context.RegisterFunction(new GetTypedMemberFactsFunction(_factFinder, _context));
    }

    private void InitializeTagProcessors()
    {
        if (_context == null)
            throw new InvalidOperationException("Context not initialized");

        // Register all tag processors
        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-fact",
            new XbrlFactTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-iterate-dimension",
            new DimensionIteratorProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-label",
            new LabelTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-variable",
            new VariableTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-set-variable",
            new SetVariableProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-presentation-linkbase",
            new PresentationLinkbaseProcessor(_configuration, _context, _factFinder, _taxonomy!));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-if",
            new ConditionalProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-else",
            new ElseProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-iterate-variable",
            new VariableIteratorProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor("hh:xbrl-iterate-typed-dimension",
            new TypedDimensionIteratorTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-role-container",
            new RoleContainerTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-taxonomy-json",
            new XbrlTaxonomyJsonTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-footnotes-container",
            new XbrlFootnotesContainerTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-footnote",
            new XbrlFootnoteTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-anchor",
            new XbrlAnchorTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterTagProcessor($"{_configuration.TemplateNamespacePrefix}:xbrl-viewer-fact-list",
            new XbrlViewerFactListTagProcessor(_configuration, _context, _factFinder));

        _context.RegisterAttributeProcessor("xbrl-label", new LabelAttributeProcessor(_configuration, _context, _factFinder));

        _context.RegisterAttributeProcessor("xbrl-role-label", new RoleLabelAttributeProcessor(_configuration, _context, _factFinder));

        _context.RegisterAttributeProcessor("localized-text", new LocalizedTextAttributeProcessor(_configuration, _context, _factFinder));

        _context.RegisterAttributeProcessor("xbrl-if", new ConditionalAttributeProcessor(_configuration, _context, _factFinder));
    }

    private async Task<IEnumerable<XmlNode>>? ProcessNode(XmlNode? node)
    {
        if (_context == null)
            throw new InvalidOperationException("Context not initialized");

        if (node == null)
            throw new ArgumentNullException(nameof(node));

        switch (node.NodeType)
        {
            case XmlNodeType.Element:
                var element = (XmlElement)node;

                // Check if it's a template tag
                var tagProcessor = _context.GetProcessorForElement(element);
                if (tagProcessor != null)
                {
                    return await tagProcessor.Process(element);
                }

                // Check if it's a template attribute
                var attributeProcessor = _context.GetProcessorForAttributes(element);
                if (attributeProcessor != null)
                {
                    return await attributeProcessor.Process(element);
                }

                // Regular element - process children
                var newElement = _context.Document.CreateElement(
                    element.Prefix,
                    element.LocalName,
                    element.NamespaceURI);

                // Copy attributes
                foreach (XmlAttribute attr in element.Attributes)
                {
                    var newAttr = _context.Document.CreateAttribute(
                        attr.Prefix,
                        attr.LocalName,
                        attr.NamespaceURI);
                    newAttr.Value = _context.EvaluateExpression(attr.Value);
                    newElement.Attributes.Append(newAttr);
                }

                // Process children
                foreach (XmlNode child in element.ChildNodes)
                {
                    var newChild = await ProcessNode(child);
                    if (newChild != null)
                        foreach (var childNode in newChild)
                            newElement.AppendChild(childNode);
                }

                return new List<XmlNode>() { newElement };

            case XmlNodeType.Text:
                // Process text nodes looking for expressions
                var text = node.InnerText;
                if (text.Contains("${"))
                {
                    // Use the context to evaluate the expression and replace the text
                    text = _context.EvaluateExpression(text);
                }
                return new List<XmlNode>() { _context.Document.CreateTextNode(text) };

            case XmlNodeType.Comment:
                if (_configuration.OutputConfiguration.IncludeDebugComments)
                {
                    return new List<XmlNode>() { _context.Document.CreateComment(node.Value ?? string.Empty) };
                }
                return new List<XmlNode>() { _context.Document.CreateTextNode(string.Empty) };

            default:
                return new List<XmlNode>() { _context.Document.CreateTextNode(string.Empty) };
        }
    }

    private void ProcessHeaderSection(XmlDocument document, TemplateContext context)
    {
        var namespaceManager = CreateNamespaceManager(document);
        var headerNode = document.SelectSingleNode("//ix:header", namespaceManager);
        if (headerNode == null)
            return;

        // Process hidden facts
        var newHeaderNode = context.ElementBuilder.CreateHeader(_instanceDocument!, context.Configuration);
        headerNode.ParentNode!.ReplaceChild(newHeaderNode, headerNode);
    }

    private void ProcessHiddenFacts(XmlNode hiddenNode)
    {
        // Implementation for processing hidden facts
    }

    private void ProcessReferences(XmlNode referencesNode)
    {
        // Implementation for processing references
    }

    private void ProcessResources(XmlNode resourcesNode)
    {
        // Implementation for processing resources
        // write the contexts and units to the document

    }

    private void ValidateOutput(XmlDocument document)
    {
        // Implement iXBRL validation logic here
        // This could include:
        // - Schema validation
        // - Context reference validation
        // - Unit reference validation
        // - Fact consistency validation
    }

    private XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
    {
        var nsManager = new XmlNamespaceManager(document.NameTable);
        nsManager.AddNamespace("ix", "http://www.xbrl.org/2013/inlineXBRL");
        nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        nsManager.AddNamespace(_configuration.TemplateNamespacePrefix, _configuration.TemplateNamespace);
        nsManager.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        nsManager.AddNamespace("link", "http://www.xbrl.org/2003/linkbase");
        nsManager.AddNamespace("iso4217", "http://www.xbrl.org/2003/iso4217");
        nsManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
        nsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        nsManager.AddNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");
        nsManager.AddNamespace("xbrldt", "http://xbrl.org/2005/xbrldt");

        if (_taxonomy != null)
        {
            foreach (var ns in _taxonomy.Prefixes)
            {
                if (nsManager.LookupNamespace(ns.Key) == null)
                {
                    nsManager.AddNamespace(ns.Key, ns.Value);
                }
            }
        }

        return nsManager;
    }
}
