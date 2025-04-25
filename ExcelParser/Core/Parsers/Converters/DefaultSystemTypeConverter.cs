using ExcelParser.Core.Abstractions;
using System.Globalization;

namespace ExcelParser.Core.Parsers.Converters;

public class DefaultSystemTypeConverter : ITypeConverter
{
    public bool CanConvert(Type targetType)
    {
        return true; // Этот конвертер обрабатывает всё остальное
    }

    public object? Convert(string value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value, ignoreCase: true);

        return System.Convert.ChangeType(value, underlyingType, CultureInfo.CurrentCulture);
    }
}
