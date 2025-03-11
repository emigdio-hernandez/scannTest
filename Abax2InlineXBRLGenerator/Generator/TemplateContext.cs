using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Maintains the state and provides utilities during template processing.
/// </summary>
public class TemplateContext
{
    private readonly Stack<Dictionary<string, TemplateVariable>> _variableScopes;
    private readonly Dictionary<string, string> _contextIdCache;
    private readonly Dictionary<string, TagProcessor> _tagProcessors;
    private readonly Dictionary<string, ITemplateFunction> _functions = new();
    private readonly Dictionary<string, AttributeProcessor> _attributeProcessors = new();
    private readonly Dictionary<string, FactViewerData> _factsViewerData;

    private int _contextCounter;
    private int _unitCounter;

    /// <summary>
    /// The element builder for creating XBRL elements
    /// </summary>
    public XbrlElementBuilder ElementBuilder { get; }

    /// <summary>
    /// The XML document being processed
    /// </summary>
    public XmlDocument Document { get; }

    /// <summary>
    /// The XBRL taxonomy for reference
    /// </summary>
    public XBRLTaxonomy Taxonomy { get; }

    /// <summary>
    /// The XBRL instance document for reference
    /// </summary>
    public XBRLInstanceDocument InstanceDocument { get; }

    /// <summary>
    /// Current nesting level in template processing
    /// </summary>
    public int CurrentNestingLevel { get; set; }

    /// <summary>
    /// Current template configuration
    /// </summary>
    public TemplateConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the TemplateContext class.
    /// </summary>
    /// <param name="configuration">Template configuration</param>
    /// <param name="document">XML document being processed</param>
    /// <param name="taxonomy">XBRL taxonomy for reference</param>
    /// <param name="instanceDocument">XBRL instance document for reference</param>
    /// <param name="defaultLanguage">The default language for labels</param>
    public TemplateContext(TemplateConfiguration configuration, XmlDocument document, XBRLTaxonomy? taxonomy, XBRLInstanceDocument? instanceDocument)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Taxonomy = taxonomy ?? throw new ArgumentNullException(nameof(taxonomy));
        ElementBuilder = new XbrlElementBuilder(document, configuration, taxonomy);
        InstanceDocument = instanceDocument ?? throw new ArgumentNullException(nameof(instanceDocument));

        _variableScopes = new Stack<Dictionary<string, TemplateVariable>>();
        _contextIdCache = new Dictionary<string, string>();
        _tagProcessors = new Dictionary<string, TagProcessor>();
        _contextCounter = 0;
        _unitCounter = 0;

        // Initialize with global scope
        PushScope();
        foreach (var (name, variable) in Configuration.GlobalVariables)
        {
            SetVariableValue(name, variable);
        }

        _factsViewerData = new Dictionary<string, FactViewerData>();
    }

    #region Variable Management

    /// <summary>
    /// Creates a new variable scope
    /// </summary>
    public void PushScope()
    {
        _variableScopes.Push(new Dictionary<string, TemplateVariable>());
    }

    /// <summary>
    /// Removes the current variable scope
    /// </summary>
    public void PopScope()
    {
        if (_variableScopes.Count <= 1)
            throw new InvalidOperationException("Cannot remove global scope");

        _variableScopes.Pop();
    }

    /// <summary>
    /// Sets a variable value in the current scope
    /// </summary>
    public void SetVariableValue(string name, TemplateVariable variable)
    {
        if (_variableScopes.Count == 0)
            throw new InvalidOperationException("No active variable scope");

        _variableScopes.Peek()[name] = variable;
    }

    /// <summary>
    /// Gets a variable value, searching through all scopes
    /// </summary>
    public TemplateVariable? GetBaseVariable(string name)
    {
        foreach (var scope in _variableScopes)
        {
            if (scope.TryGetValue(name, out var variable))
                return variable;
        }
        return null;
    }

    /// <summary>
    /// Gets a variable value, searching through all scopes and resolving dictionary properties
    /// </summary>
    public TemplateVariable? GetVariable(string name)
    {
        // Check if we're accessing a dictionary property using square brackets
        var bracketMatch = Regex.Match(name, @"^([^[\]]+)\[([""]|\\['""]\s*)([^""']+?)(\2|\s*\\['""]\])\]$");
        if (bracketMatch.Success)
        {
            var dictionaryName = bracketMatch.Groups[1].Value;
            var propertyName = bracketMatch.Groups[3].Value;

            // Limpiar las comillas escapadas si existen
            propertyName = propertyName.Replace("\\'", "'").Replace("\\\"", "\"");

            // Get the base variable (dictionary)
            var baseVariable = GetBaseVariable(dictionaryName);
            if (baseVariable?.Type != TemplateVariable.VariableType.Dictionary)
                return null;

            // Get the dictionary value
            if (baseVariable.GetValue() is IDictionary<string, string> dict)
            {
                // Try to get the value from the dictionary
                if (dict.TryGetValue(propertyName, out var value))
                {
                    // Create a new variable with the dictionary value
                    var newVar = new TemplateVariable(
                        propertyName,
                        TemplateVariable.VariableType.String,
                        baseVariable.Scope,
                        baseVariable.ContextId);
                    newVar.SetValue(value);
                    return newVar;
                }
            }
            return null;
        }

        // Existing array access check
        var arrayMatch = Regex.Match(name, @"^([^[\]]+)\[(\d+)\](?:\.(\w+))?$");
        if (arrayMatch.Success)
        {
            var arrayName = arrayMatch.Groups[1].Value;
            var arrayIndex = int.Parse(arrayMatch.Groups[2].Value);
            var propertyName = arrayMatch.Groups[3].Success ? arrayMatch.Groups[3].Value : null;

            // Get the array variable
            var arrayVariable = GetBaseVariable(arrayName);
            if (arrayVariable?.Type != TemplateVariable.VariableType.DictionaryArray)
                return null;

            var dictArray = arrayVariable.GetValue<Dictionary<string, string>[]>();
            if (dictArray == null || arrayIndex >= dictArray.Length)
                return null;

            var dictionary = dictArray[arrayIndex];

            // If no property specified, return the whole dictionary
            if (propertyName == null)
            {
                var newVar = new TemplateVariable(
                    name,
                    TemplateVariable.VariableType.Dictionary,
                    arrayVariable.Scope,
                    arrayVariable.ContextId);
                newVar.SetValue(dictionary);
                return newVar;
            }

            // Get the property from the dictionary
            if (dictionary.TryGetValue(propertyName, out var value))
            {
                var newVar = new TemplateVariable(
                    name,
                    TemplateVariable.VariableType.String,
                    arrayVariable.Scope,
                    arrayVariable.ContextId);
                newVar.SetValue(value);
                return newVar;
            }

            return null;
        }

        // Check if we're accessing a dictionary property using dot notation
        var parts = name.Split('.');
        if (parts.Length > 1)
        {
            // Get the base variable (dictionary)
            var baseVariable = GetBaseVariable(parts[0]);
            if (baseVariable == null)
                return null;

            if (baseVariable?.Type != TemplateVariable.VariableType.Dictionary)
                return null;

            // Get the dictionary value
            if (baseVariable.GetValue() is IDictionary<string, string> dict)
            {
                // Try to get the value from the dictionary
                if (dict.TryGetValue(parts[1], out var value))
                {
                    // Create a new variable with the dictionary value
                    var newVar = new TemplateVariable(
                        parts[1],
                        TemplateVariable.VariableType.String,
                        baseVariable.Scope,
                        baseVariable.ContextId);
                    newVar.SetValue(value);
                    return newVar;
                }
            }
            return null;
        }

        // Normal variable lookup
        foreach (var scope in _variableScopes)
        {
            if (scope.TryGetValue(name, out var variable))
                return variable;
        }
        return null;
    }

    /// <summary>
    /// Gets the string value of a variable
    /// </summary>
    public string? GetVariableValue(string name)
    {
        var variable = GetVariable(name);
        return variable?.GetValue<string>();
    }

    #endregion

    #region Context Management

    /// <summary>
    /// Gets or generates a context ID for a fact
    /// </summary>
    public string GetContextId(XBRLFact fact)
    {
        var hash = XBRLFact.Hash(fact.Concept, fact.Unit != null ? fact.Unit.Id : null, fact.Period!.Id!, fact.Entity!, fact.Dimensions);

        if (!_contextIdCache.TryGetValue(hash, out var contextId))
        {
            contextId = string.Format(Configuration.OutputConfiguration.ContextIdFormat, ++_contextCounter);
            _contextIdCache[hash] = contextId;
        }

        return contextId;
    }

    /// <summary>
    /// Generates a new unit ID
    /// </summary>
    public string GenerateUnitId()
    {
        return string.Format(Configuration.OutputConfiguration.UnitIdFormat, ++_unitCounter);
    }

    #endregion

    # region Function Management

    /// <summary>
    /// Registers a function in the context
    /// </summary>
    /// <param name="function">The function to register</param>
    public void RegisterFunction(ITemplateFunction function)
    {
        _functions[function.Name] = function;
    }

    /// <summary>
    /// Gets a function by its name
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <returns>The function or null if it doesn't exist</returns>
    public ITemplateFunction? GetFunction(string name)
    {
        return _functions.TryGetValue(name, out var function) ? function : null;
    }

    /// <summary>
    /// Gets all registered functions
    /// </summary>
    /// <returns>A dictionary of functions</returns>
    public IDictionary<string, ITemplateFunction> GetFunctions()
    {
        return _functions;
    }

    /// <summary>
    /// Sets the functions in the context
    /// </summary>
    /// <param name="functions">The functions to set</param>
    public void SetFunctions(IDictionary<string, ITemplateFunction> functions)
    {
        foreach (var (name, function) in functions)
        {
            if (_functions.ContainsKey(name))
                _functions[name] = function;
            else
                _functions.Add(name, function);
        }
    }

    #endregion

    #region Tag Processing

    /// <summary>
    /// Registers a tag processor for a specific element name
    /// </summary>
    public void RegisterTagProcessor(string elementName, TagProcessor processor)
    {
        _tagProcessors[elementName] = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    /// <summary>
    /// Gets the appropriate processor for an element
    /// </summary>
    public TagProcessor? GetProcessorForElement(XmlElement element)
    {
        return _tagProcessors.TryGetValue($"{element.Prefix}:{element.LocalName}", out var processor) ? processor : null;
    }

    /// <summary>
    /// Registers an attribute processor for a specific attribute name
    /// </summary>
    public void RegisterAttributeProcessor(string attributeName, AttributeProcessor processor)
    {
        _attributeProcessors[attributeName] = processor;
    }

    /// <summary>
    /// Gets the appropriate attribute processor for the attributes of an element
    /// </summary>
    public AttributeProcessor? GetProcessorForAttributes(XmlElement element)
    {
        foreach (XmlAttribute attr in element.Attributes)
        {
            if (attr.NamespaceURI == Configuration.TemplateNamespace &&
                _attributeProcessors.TryGetValue(attr.LocalName, out var processor))
            {
                return processor;
            }
        }
        return null;
    }

    #endregion

    #region Expression Evaluation

    /// <summary>
    /// Evaluates an expression containing variable references, function invocations and ternary operators.
    /// </summary>
    /// <param name="expressionToEvaluate">The expression to evaluate</param>
    /// <returns>The evaluated expression value</returns>
    public string EvaluateExpression(string expressionToEvaluate)
    {
        if (!expressionToEvaluate.Contains("${"))
            return expressionToEvaluate;

        // First, evaluate all innermost expressions
        while (expressionToEvaluate.Contains("${"))
        {
            // Regular expression to find the innermost expression
            var innerMatch = Regex.Match(
                expressionToEvaluate,
                @"\${([^${}]*(?:(?:\{(?:[^{}]|(?<open>\{)|(?<-open>\}))*\})|[^${}])*)}",
                RegexOptions.Compiled | RegexOptions.Singleline
            );

            if (!innerMatch.Success)
                break;

            var fullMatch = innerMatch.Value;
            var expression = innerMatch.Groups[1].Value;

            // Check if it's a ternary operation
            if (expression.Contains("?"))
            {
                var ternaryMatch = Regex.Match(expression, @"(.+?)\s*\?\s*(.+?)\s*:\s*(.+)");
                if (ternaryMatch.Success)
                {
                    var condition = ternaryMatch.Groups[1].Value;
                    var trueValue = ternaryMatch.Groups[2].Value;
                    var falseValue = ternaryMatch.Groups[3].Value;

                    // Evaluar la condición y los valores correspondientes
                    var ternaryEvaluatedValue = EvaluateBoolean(condition)
                        ? EvaluateValue(trueValue)
                        : EvaluateValue(falseValue);

                    expressionToEvaluate = expressionToEvaluate.Replace(fullMatch, ternaryEvaluatedValue);
                    continue;
                }
            }

            // Check if it's an arithmetic operation
            if (ContainsArithmeticOperator(expression))
            {
                var evaluatedArithmeticValue = EvaluateValue(expression);
                expressionToEvaluate = expressionToEvaluate.Replace(fullMatch, evaluatedArithmeticValue);
                continue;
            }

            // If itsn't an arithmetic operation nor a ternary operation, evaluate the expression
            var evaluatedValue = EvaluateValue("${" + expression + "}");
            expressionToEvaluate = expressionToEvaluate.Replace(fullMatch, evaluatedValue);
        }

        return expressionToEvaluate;
    }

    /// <summary>
    /// Evaluates a boolean expression
    /// </summary>
    public bool EvaluateBoolean(string expression)
    {
        // Remove whitespace
        expression = Regex.Replace(expression, @"\s+", "");

        // Handle not contains operator (debe ir antes que contains)
        if (expression.Contains("!contains"))
        {
            var parts = expression.Split("!contains", 2);
            var leftValue = EvaluateValue("${" + parts[0] + "}");
            var rightValue = EvaluateValue("${" + parts[1] + "}");
            
            // Remover comillas si existen
            leftValue = RemoveQuotes(leftValue);
            rightValue = RemoveQuotes(rightValue);
            
            return !leftValue.Contains(rightValue, StringComparison.OrdinalIgnoreCase);
        }

        // Handle contains operator
        if (expression.Contains("contains"))
        {
            var parts = expression.Split("contains", 2);
            var leftValue = EvaluateValue("${" + parts[0] + "}");
            var rightValue = EvaluateValue("${" + parts[1] + "}");
            
            // Remover comillas si existen
            leftValue = RemoveQuotes(leftValue);
            rightValue = RemoveQuotes(rightValue);
            
            return leftValue.Contains(rightValue, StringComparison.OrdinalIgnoreCase);
        }

        // Handle simple equality
        if (expression.Contains("=="))
        {
            var parts = expression.Split("==", 2);
            return EvaluateValue("${" + parts[0] + "}") == EvaluateValue("${" + parts[1] + "}");
        }

        // Handle inequality
        if (expression.Contains("!="))
        {
            var parts = expression.Split("!=", 2);
            return EvaluateValue("${" + parts[0] + "}") != EvaluateValue("${" + parts[1] + "}");
        }

        // Handle greater than or equal
        if (expression.Contains(">="))
        {
            var parts = expression.Split(">=", 2);
            return CompareValues("${" + parts[0] + "}", "${" + parts[1] + "}") >= 0;
        }

        // Handle less than or equal
        if (expression.Contains("<="))
        {
            var parts = expression.Split("<=", 2);
            return CompareValues("${" + parts[0] + "}", "${" + parts[1] + "}") <= 0;
        }

        // Handle greater than
        if (expression.Contains(">"))
        {
            var parts = expression.Split(">", 2);
            return CompareValues("${" + parts[0] + "}", "${" + parts[1] + "}") > 0;
        }

        // Handle less than
        if (expression.Contains("<"))
        {
            var parts = expression.Split("<", 2);
            return CompareValues("${" + parts[0] + "}", "${" + parts[1] + "}") < 0;
        }

        // Handle logical AND
        if (expression.Contains("&&"))
        {
            var parts = expression.Split("&&", 2);
            return EvaluateBoolean("${" + parts[0] + "}") && EvaluateBoolean("${" + parts[1] + "}");
        }

        // Handle logical OR
        if (expression.Contains("||"))
        {
            var parts = expression.Split("||", 2);
            return EvaluateBoolean("${" + parts[0] + "}") || EvaluateBoolean("${" + parts[1] + "}");
        }

        // Handle logical NOT
        if (expression.StartsWith("!"))
        {
            return !EvaluateBoolean(expression[1..]);
        }

        if (bool.TryParse(expression, out var oresult) && oresult)
        {
            return true;
        }

        // Evaluate as a variable expression that should be of type boolean
        var value = EvaluateValue(expression.Contains("${") ? expression : "${" + expression + "}");
        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        return false;
    }

    private string EvaluateValue(string value)
    {
        try
        {
            // Set a timeout for regex operations
            var regexTimeout = TimeSpan.FromSeconds(1);
            // Remove unnecessary whitespace
            value = Regex.Replace(value.Trim(), @"\s+", " ");

            // Only evaluate if it contains an arithmetic operator
            if (ContainsArithmeticOperator(value))
            {
                try
                {
                    // Check if it's an arithmetic operation
                    var operatorMatch = Regex.Match(value, @"^(?:[^""']+|""[^""]*""|'[^']*')+?([+\-*/%^])(.+)$", RegexOptions.None, regexTimeout);
                    if (operatorMatch.Success)
                    {
                        var lastOperatorIndex = operatorMatch.Groups[1].Index;
                        var leftValue = EvaluateValue(value[..lastOperatorIndex].Trim());
                        var op = operatorMatch.Groups[1].Value;
                        var rightValue = EvaluateValue(value[(lastOperatorIndex + 1)..].Trim());

                        // Check if it's a number
                        var leftIsNumber = double.TryParse(leftValue, out var leftNumber);
                        var rightIsNumber = double.TryParse(rightValue, out var rightNumber);

                        if (!leftIsNumber && !rightIsNumber)
                        {
                            // Special case: string concatenation
                            if (op == "+")
                            {
                                leftValue = RemoveQuotes(leftValue);
                                rightValue = RemoveQuotes(rightValue);
                                return leftValue + rightValue;
                            }
                            else
                            {
                                throw new TemplateProcessingException(
                                    $"Cannot perform arithmetic operation on non-numeric values: {leftValue} {op} {rightValue}");
                            }
                        }
                        else if (leftIsNumber && rightIsNumber)
                        {
                            var result = op switch
                            {
                                "+" => leftNumber + rightNumber,
                                "-" => leftNumber - rightNumber,
                                "*" => leftNumber * rightNumber,
                                "/" => rightNumber != 0
                                    ? leftNumber / rightNumber
                                    : throw new DivideByZeroException("Division by zero"),
                                "%" => leftNumber % rightNumber,
                                "^" => Math.Pow(leftNumber, rightNumber),
                                _ => throw new TemplateProcessingException($"Unsupported operator: {op}")
                            };

                            return result.ToString(CultureInfo.InvariantCulture);
                        }
                        else if (op == "+")
                        {
                            if (leftIsNumber)
                            {
                                rightValue = RemoveQuotes(rightValue);
                            }
                            if (rightIsNumber)
                            {
                                leftValue = RemoveQuotes(leftValue);
                            }
                            return "\"" + leftValue + rightValue + "\"";
                        }
                        else
                        {
                            throw new TemplateProcessingException(
                                $"Cannot perform arithmetic operation on mixed numeric and non-numeric values: {leftValue} {op} {rightValue}");
                        }
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // if the regex operation times out, ignore it and continue
                }
            }

            // if there's not an arithmetic operation, check if it's a function call
            try
            {
                var functionMatch = Regex.Match(value, @"(\w+)\((.*)\)", RegexOptions.None, regexTimeout);
                if (functionMatch.Success)
                {
                    var functionName = functionMatch.Groups[1].Value;
                    var arguments = ParseFunctionArguments(functionMatch.Groups[2].Value);

                    if (!_functions.TryGetValue(functionName, out var function))
                    {
                        throw new TemplateProcessingException($"Unknown function: {functionName}");
                    }

                    try
                    {
                        var result = function.Execute(arguments.ToArray());
                        return result?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        throw new TemplateProcessingException($"Error executing function {functionName}", ex);
                    }
                }

            }
            catch (RegexMatchTimeoutException)
            {
                // if the regex operation times out, ignore it and continue
            }

            // Check if it's a variable reference or direct value
            if (value.StartsWith("${") && value.EndsWith("}"))
            {
                var innerValue = value[2..^1];

                // first, check if it's a variable
                var variableValue = GetVariableValue(innerValue);
                if (variableValue != null)
                    return variableValue;

                // if it's not a variable, try as a number
                if (double.TryParse(innerValue, out _))
                    return innerValue;

                // If it's a string, remove outer quotes
                if (innerValue.StartsWith("\"") && innerValue.EndsWith("\"") ||
                    innerValue.StartsWith("'") && innerValue.EndsWith("'"))
                {
                    return innerValue[1..^1];
                }

                // if it's not a number nor a string, return inner value as is
                return innerValue;
            }

            return RemoveQuotes(value);
        }
        catch (Exception ex) when (ex is not TemplateProcessingException)
        {
            // Log del error si es necesario
            throw new TemplateProcessingException($"Error evaluating expression: {value}", ex);
        }
    }



    private string RemoveQuotes(string value)
    {
        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            return value[1..^1];
        }
        return value;
    }

    private bool ContainsArithmeticOperator(string value)
    {
        // Check if the value contains any arithmetic operator outside of quotes
        bool inQuotes = false;
        int braceCount = 0;
        char quoteChar = '\0';

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            // If we're inside a JSON (braceCount > 0), ignore everything until we find the closing brace
            if (braceCount > 0)
            {
                if (c == '{') braceCount++;
                if (c == '}') braceCount--;
                continue;
            }

            // Detect start of JSON
            if (c == '{')
            {
                braceCount++;
                continue;
            }

            // Handle quotes only if not in JSON
            if (c == '"' || c == '\'')
            {
                if (!inQuotes)
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (c == quoteChar)
                {
                    inQuotes = false;
                }
                continue;
            }

            // Check for arithmetic operators only when not in quotes or JSON
            if (!inQuotes && braceCount == 0 && "+-*/%^".Contains(c))
            {
                return true;
            }
        }

        return false;
    }


    private List<object> ParseFunctionArguments(string argumentsString)
    {
        var arguments = new List<object>();
        if (string.IsNullOrWhiteSpace(argumentsString))
            return arguments;

        var currentArg = new StringBuilder();
        var inQuotes = false;
        var braceCount = 0;
        var depth = 0;
        char quoteChar = '\0';

        for (var i = 0; i < argumentsString.Length; i++)
        {
            var c = argumentsString[i];

            // Si estamos dentro de un JSON, acumular caracteres hasta encontrar la llave de cierre correspondiente
            if (braceCount > 0)
            {
                if (c == '{') braceCount++;
                if (c == '}') braceCount--;
                currentArg.Append(c);
                continue;
            }

            // Detectar inicio de JSON
            if (c == '{')
            {
                braceCount++;
                currentArg.Append(c);
                continue;
            }

            switch (c)
            {
                case '"' or '\'' when i == 0 || argumentsString[i - 1] != '\\':
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuotes = false;
                    }
                    currentArg.Append(c);
                    break;

                case '(' when !inQuotes:
                    depth++;
                    currentArg.Append(c);
                    break;

                case ')' when !inQuotes:
                    depth--;
                    currentArg.Append(c);
                    break;

                case ',' when !inQuotes && depth == 0 && braceCount == 0:
                    arguments.Add(EvaluateArgumentValue(currentArg.ToString().Trim()));
                    currentArg.Clear();
                    break;

                default:
                    currentArg.Append(c);
                    break;
            }
        }

        if (currentArg.Length > 0)
        {
            arguments.Add(EvaluateArgumentValue(currentArg.ToString().Trim()));
        }

        return arguments;
    }

    private object EvaluateArgumentValue(string arg)
    {
        // Check if it's a variable or function call
        if (arg.Contains("${") || arg.Contains("("))
        {
            return EvaluateExpression(arg);
        }

        // Remove outer quotes if present
        if (arg.StartsWith("'") && arg.EndsWith("'") ||
            arg.StartsWith("\"") && arg.EndsWith("\""))
        {
            return arg[1..^1];
        }

        // Check if it's a number
        if (decimal.TryParse(arg, out var number))
        {
            return number;
        }

        // Check if it's a boolean
        if (bool.TryParse(arg, out var boolean))
        {
            return boolean;
        }

        return arg;
    }

    private int CompareValues(string left, string right)
    {
        var leftValue = EvaluateValue(left);
        var rightValue = EvaluateValue(right);

        // Try compare as numbers first
        if (decimal.TryParse(leftValue, out var leftNumber) &&
            decimal.TryParse(rightValue, out var rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        // Fall back to string comparison
        return string.Compare(leftValue, rightValue, StringComparison.Ordinal);
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Logs an error or throws an exception depending on configuration
    /// </summary>
    public void HandleError(string message, Exception? exception = null)
    {
        if (Configuration.StrictValidation)
        {
            throw new TemplateProcessingException(message, exception);
        }

        // Log the error but continue processing
        // Implement logging according to your needs
    }

    #endregion

    #region FactViewerData Management

    /// <summary>
    /// Adds FactViewerData to the context
    /// </summary>
    /// <param name="factId">The ID of the fact</param>
    /// <param name="factData">The FactViewerData to add</param>
    public void AddFactViewerData(string factId, FactViewerData factData)
    {
        _factsViewerData[factId] = factData;
    }

    /// <summary>
    /// Gets all FactViewerData stored in the context
    /// </summary>
    /// <returns>A dictionary of FactViewerData</returns>
    public Dictionary<string, FactViewerData> GetFactsViewerData()
    {
        return _factsViewerData;
    }

    #endregion
}
