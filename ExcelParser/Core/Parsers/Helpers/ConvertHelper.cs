using ExcelParser.Core.Parsers.Converters;
using System.Globalization;

namespace ExcelParser.Core.Parsers.Helpers;

/// <summary>
/// Помощник для конвертации строковых значений в нужные типы.
/// </summary>
public static class ConvertHelper
{
    public static object? ConvertTo(string value, Type targetType)
    {
        return DefaultTypeConverters.Convert(value, targetType);
    }
}