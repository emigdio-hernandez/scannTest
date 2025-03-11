
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents a template variable that can be used in a template document.
/// </summary>
public class TemplateVariable
{
    /// <summary>
    /// Data type of the variable.
    /// </summary>
    public enum VariableType
    {
        String,
        Double,
        Boolean,
        Dictionary,
        DictionaryArray,  // Array of dictionaries with string keys and values
        StringArray  // Array of strings
    }

    /// <summary>
    /// Scope of the variable.
    /// </summary>
    public enum VariableScope
    {
        Global,    // Variable available in all contexts
        Local,     // Variable defined in a specific context
        Iterator   // Variable defined in a loop context
    }

    /// <summary>
    /// Unique name of the variable.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Data type of the variable.
    /// </summary>
    public VariableType Type { get; private set; }

    /// <summary>
    /// Scope of the variable.
    /// </summary>
    public VariableScope Scope { get; private set; }

    /// <summary>
    /// Value of the variable.
    /// </summary>
    private object? _value;

    /// <summary>
    /// The ID of the context where the variable is defined.
    /// </summary>
    public string? ContextId { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateVariable"/> class.
    /// </summary>
    /// <param name="name">The unique name of the variable.</param>
    /// <param name="type">The data type of the variable.</param>
    /// <param name="scope">The scope of the variable.</param>
    /// <param name="contextId">The ID of the context where the variable is defined.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
    public TemplateVariable(string name, VariableType type, VariableScope scope, string? contextId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Scope = scope;
        ContextId = contextId;
        ValidateName(name);
    }

    /// <summary>
    /// Sets the value of the variable.
    /// </summary>
    public void SetValue(object? value)
    {
        if (value == null)
        {
            _value = null;
            return;
        }

        // Convert the value to the appropriate type
        _value = Type switch
        {
            VariableType.String => ConvertToString(value),
            VariableType.Double => ConvertToDouble(value),
            VariableType.Boolean => ConvertToBoolean(value),
            VariableType.Dictionary => ConvertToDictionary(value),
            VariableType.DictionaryArray => ConvertToDictionaryArray(value),
            VariableType.StringArray => ConvertToStringArray(value),
            _ => throw new InvalidOperationException($"Tipo de variable no soportado: {Type}")
        };
    }

    /// <summary>
    /// Gets the value of the variable as the specified type.
    /// </summary>
    public T? GetValue<T>()
    {
        if (_value == null)
        {
            return default;
        }

        try
        {
            return (T)Convert.ChangeType(_value, typeof(T));
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException(
                $"No se puede convertir el valor de tipo {_value.GetType()} a {typeof(T)}");
        }
    }

    /// <summary>
    /// Gets the value of the variable.
    /// </summary>
    public object? GetValue() => _value;

    /// <summary>
    /// Whether the variable has a value.
    /// </summary>
    public bool HasValue => _value != null;

    /// <summary>
    /// Clones the variable with a new context ID.
    /// </summary>
    public TemplateVariable Clone(string? newContextId)
    {
        var clone = new TemplateVariable(Name, Type, Scope, newContextId);
        clone._value = Type == VariableType.DictionaryArray
            ? CloneDictionaryArray((Dictionary<string, string>[]?)_value)
            : Type == VariableType.Dictionary
                ? CloneDictionary((Dictionary<string, string>?)_value)
                : _value;
        return clone;
    }

    #region Private Methods

    /// <summary>
    /// Validates the name of the variable.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <exception cref="ArgumentException">Thrown when the name is invalid.</exception>
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty");
        }

        if (!char.IsLetter(name[0]) && name[0] != '_')
        {
            throw new ArgumentException("Name must start with a letter or underscore");
        }

        if (!name.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            throw new ArgumentException("Name must contain only letters, digits, or underscores");
        }
    }

    /// <summary>
    /// Converts the value to a string.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The value as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    private static string ConvertToString(object value)
    {
        return value?.ToString() ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Converts the value to a double.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The value as a double.</returns>
    /// <exception cref="ArgumentException">Thrown when the value cannot be converted to a double.</exception>
    private static double ConvertToDouble(object value)
    {
        try
        {
            return Convert.ToDouble(value);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new ArgumentException($"No se puede convertir '{value}' a Double", ex);
        }
    }

    /// <summary>
    /// Converts the value to a boolean.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The value as a boolean.</returns>
    /// <exception cref="ArgumentException">Thrown when the value cannot be converted to a boolean.</exception>
    private static bool ConvertToBoolean(object value)
    {
        if (value is string stringValue)
        {
            if (bool.TryParse(stringValue, out bool result))
            {
                return result;
            }
            // Permitir "1" y "0" como valores booleanos
            if (stringValue == "1") return true;
            if (stringValue == "0") return false;
        }

        try
        {
            return Convert.ToBoolean(value);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new ArgumentException($"No se puede convertir '{value}' a Boolean", ex);
        }
    }

    /// <summary>
    /// Converts the given object to a dictionary with string keys and values.
    /// </summary>
    /// <param name="value">The object to convert. It can be a JSON string, a dictionary, or an enumerable of key-value pairs.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private Dictionary<string, string>? ConvertToDictionary(object value)
    {
        try
        {
            return value switch
            {
                Dictionary<string, string> dict => dict,
                string jsonStr => JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr),
                IDictionary<string, string> dict => new Dictionary<string, string>(dict),
                _ => throw new ArgumentException($"No se puede convertir '{value}' a Dictionary<string, string>")
            };
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("El formato JSON proporcionado no es válido para un diccionario", ex);
        }
    }

    /// <summary>
    /// Converts the given object to an array of strings.
    /// </summary>
    /// <param name="value">The object to convert. It can be a JSON string, an array of strings, or an enumerable of strings.</param>
    /// <returns>An array of strings.</returns>
    /// <exception cref="ArgumentException">when the input object cannot be converted to an array of strings or when the JSON format is invalid.</exception>
    /// <exception cref="JsonException">when the JSON string cannot be deserialized into an array of strings.</exception>
    private string[]? ConvertToStringArray(object value)
    {
        try
        {
            return value switch
            {
                string[] array => array,
                string jsonStr => JsonConvert.DeserializeObject<string[]>(jsonStr),
                IEnumerable<string> enumerable => enumerable.ToArray(),
                _ => throw new ArgumentException($"No se puede convertir '{value}' a array de strings")
            };
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("El formato JSON proporcionado no es válido para un array de strings", ex);
        }
    }

    /// <summary>
    /// Converts the given object to an array of dictionaries with string keys and values.
    /// </summary>
    /// <param name="value">The object to convert. It can be a JSON string, an array of dictionaries, or an enumerable of dictionaries.</param>
    /// <returns>An array of dictionaries with string keys and values.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input object cannot be converted to an array of dictionaries or when the JSON format is invalid.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown when the JSON string cannot be deserialized into an array of dictionaries.
    /// </exception>
    private static Dictionary<string, string>[]? ConvertToDictionaryArray(object value)
    {
        try
        {
            return value switch
            {
                Dictionary<string, string>[] array => array,
                string jsonStr => JsonConvert.DeserializeObject<Dictionary<string, string>[]>(jsonStr),
                IEnumerable<Dictionary<string, string>> enumerable => enumerable.ToArray(),
                _ => throw new ArgumentException($"Cannot convert '{value}' to an array of dictionaries")
            };
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("The provided JSON string is invalid, it must be an array of dictionaries", ex);
        }
    }

    /// <summary>
    /// Creates a deep copy of an array of dictionaries, where each dictionary is cloned with a case-insensitive string comparer.
    /// </summary>
    /// <param name="original">The original array of dictionaries to be cloned. Can be null.</param>
    /// <returns>
    /// A new array of dictionaries that is a deep copy of the original array, or null if the original array is null.
    /// </returns>
    private static Dictionary<string, string>[]? CloneDictionaryArray(Dictionary<string, string>[]? original)
    {
        if (original == null) return null;

        return original.Select(dict =>
            new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase)
        ).ToArray();
    }

    /// <summary>
    /// Creates a deep copy of a dictionary with a case-insensitive string comparer.
    /// </summary>
    /// <param name="original">The original dictionary to be cloned. Can be null.</param>
    /// <returns>a new dictionary that is a deep copy of the original dictionary, or null if the original dictionary is null.</returns>
    private static Dictionary<string, string>? CloneDictionary(Dictionary<string, string>? original)
    {
        if (original == null) return null;
        return new Dictionary<string, string>(original, StringComparer.OrdinalIgnoreCase);
    }

    #endregion
}