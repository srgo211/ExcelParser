using ExcelParser.Core.Abstractions;

namespace ExcelParser.Core.Parsers.Converters;

/// <summary>
/// Реестр встроенных конвертеров типов.
/// </summary>
public static class DefaultTypeConverters
{
    private static readonly List<ITypeConverter> Converters = new()
    {
        new BoolTypeConverter(),
        new DateTimeTypeConverter(),
        new DecimalTypeConverter(),
        new DefaultSystemTypeConverter()
    };

    public static object? Convert(string value, Type targetType)
    {
        foreach (var converter in Converters)
        {
            if (converter.CanConvert(targetType))
            {
                return converter.Convert(value, targetType);
            }
        }

        throw new InvalidOperationException($"Не найден конвертер для типа {targetType.Name}");
    }
}