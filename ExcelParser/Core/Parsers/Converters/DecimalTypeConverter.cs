using ExcelParser.Core.Abstractions;
using System.Globalization;

namespace ExcelParser.Core.Parsers.Converters;

/// <summary>
/// Конвертер для типов decimal, double, float.
/// Учитывает различные разделители дробной части (точка и запятая).
/// </summary>
public class DecimalTypeConverter : ITypeConverter
{
    public bool CanConvert(Type targetType)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return type == typeof(decimal) || type == typeof(double) || type == typeof(float);
    }

    public object? Convert(string value, Type targetType)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInvariant))
            return System.Convert.ChangeType(resultInvariant, type);

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var resultCurrent))
            return System.Convert.ChangeType(resultCurrent, type);

        return null;
    }
}