using ExcelParser.Core.Abstractions;

public class BoolTypeConverter : ITypeConverter
{
    private static readonly HashSet<string> TrueValues  = new(StringComparer.OrdinalIgnoreCase) { "true", "yes", "да", "1", "y", "истина" };
    private static readonly HashSet<string> FalseValues = new(StringComparer.OrdinalIgnoreCase) { "false", "no", "нет", "0", "n", "ложь" };

    public bool CanConvert(Type targetType)
    {
        return targetType == typeof(bool) || targetType == typeof(bool?);
    }

    public object? Convert(string value, Type targetType)
    {
        if (TrueValues.Contains(value))
            return true;

        if (FalseValues.Contains(value))
            return false;

        return null;
    }
}