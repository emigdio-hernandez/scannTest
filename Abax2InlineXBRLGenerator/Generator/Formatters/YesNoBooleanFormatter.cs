namespace Abax2InlineXBRLGenerator.Generator.Formatters;

public class YesNoBooleanFormatter : BaseValueFormatter
{
    public override string Format(string value)
    {
        bool booleanValue = false;
        if (bool.TryParse(value, out booleanValue))
        {
            return booleanValue ? "Yes" : "No";
        }
        return value;
    }

    public override string? XbrlFormatAlias(string? value)
    {
        bool booleanValue = false;
        if (bool.TryParse(value, out booleanValue))
        {
            return booleanValue ? "ixt:fixed-true" : "ixt:fixed-false";
        }
        return base.XbrlFormatAlias(value);
    }
}