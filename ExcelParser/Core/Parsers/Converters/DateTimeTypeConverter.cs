using ExcelParser.Core.Abstractions;
using System.Globalization;

public class DateTimeTypeConverter : ITypeConverter
{
    public bool CanConvert(Type targetType)
    {
        return targetType == typeof(DateTime) || targetType == typeof(DateTime?);
    }

    public object? Convert(string value, Type targetType)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var resultCurrent))
            return resultCurrent;

        return null;
    }
}