using System.Globalization;

namespace ExcelParser.Core.Parsers.Helpers;

/// <summary>
/// Помощник для конвертации строковых значений в нужные типы.
/// </summary>
public static class ConvertHelper
{
    public static object? ConvertTo(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (string.IsNullOrWhiteSpace(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, value, ignoreCase: true);
        }

        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            //Пробуем через (. точка)
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInvariant))
                return Convert.ChangeType(resultInvariant, underlyingType);
            //Пробуем через (локальная настройка, может быть ,)
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var resultCurrent))
                return Convert.ChangeType(resultCurrent, underlyingType);
        }

        //Конвертируем стандартно через локаль
        return Convert.ChangeType(value, underlyingType, CultureInfo.CurrentCulture);
    }
}